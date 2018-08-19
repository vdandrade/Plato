﻿using System.Threading.Tasks;
using Plato.Categories.Services;
using Plato.Categories.Stores;
using Plato.Discuss.Channels.Models;
using Plato.Entities.Models;
using Plato.Internal.Messaging.Abstractions;

namespace Plato.Discuss.Channels.Subscribers
{

    /// <summary>
    /// Updates category meta data whenever an entity is created.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntitySubscriber<TEntity> : IBrokerSubscriber where TEntity : class, IEntity
    {

        private readonly IBroker _broker;
        private readonly ICategoryStore<Channel> _channelStore;
        private readonly ICategoryManager<Channel> _channelManager;

        public EntitySubscriber(
            IBroker broker,
            ICategoryStore<Channel> channelStore,
            ICategoryManager<Channel> channelManager)
        {
            _broker = broker;
            _channelStore = channelStore;
            _channelManager = channelManager;
        }

        #region "Implementation"

        public void Subscribe()
        {
            // Created
            _broker.Sub<TEntity>(new MessageOptions()
            {
                Key = "EntityCreated"
            }, async message => await EntityCreated(message.What));
        }

        public void Unsubscribe()
        {
            _broker.Unsub<TEntity>(new MessageOptions()
            {
                Key = "EntityCreated"
            }, async message => await EntityCreated(message.What));
        }
        
        public void Dispose()
        {
            Unsubscribe();
        }

        #endregion

        #region "Private Methods"

        async Task<TEntity> EntityCreated(TEntity entity)
        {

            // No need to update cateogry for private entities
            if (entity.IsPrivate)
            {
                return entity;
            }

            // No need to update cateogry for soft deleted entities
            if (entity.IsDeleted)
            {
                return entity;
            }

            // No need to update cateogry for entities flagged as spam
            if (entity.IsSpam)
            {
                return entity;
            }

            // Ensure we have a categoryId for the newly created entity
            if (entity.CategoryId <= 0)
            {
                return entity;
            }

            // Ensure we found the category
            var channel = await _channelStore.GetByIdAsync(entity.CategoryId);
            if (channel == null)
            {
                return entity;
            }

            // Get current channel and all parent channels
            var parents = await _channelStore.GetParentsByIdAsync(channel.Id);

            // Update details within current and all parents
            foreach (var parent in parents)
            {

                // Update details with latest entity details
                var details = parent.GetOrCreate<ChannelDetails>();
                details.TotalTopics = details.TotalTopics + 1;
                details.LastPost.EntityId = entity.Id;
                details.LastPost.CreatedUserId = entity.CreatedUserId;
                details.LastPost.CreatedDate = entity.CreatedDate;
                parent.AddOrUpdate<ChannelDetails>(details);

                // Save the updated details 
                await _channelManager.UpdateAsync(parent);

            }

            return entity;

        }
  

        #endregion

    }

}
