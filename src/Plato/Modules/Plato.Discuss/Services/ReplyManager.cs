﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Plato.Discuss.Models;
using Plato.Entities.Models;
using Plato.Entities.Services;
using Plato.Entities.Stores;
using Plato.Internal.Data.Abstractions;

namespace Plato.Discuss.Services
{

    public class ReplyManager : IPostManager<EntityReply>
    {

        private readonly IEntityStore<Entity> _entityStore;
        private readonly IEntityReplyManager<EntityReply> _entityReplyManager;
        private readonly IEntityReplyStore<EntityReply> _entityReplyStore;

        public ReplyManager(IEntityReplyManager<EntityReply> entityReplyManager, IEntityReplyStore<EntityReply> entityReplyStore, IEntityStore<Entity> entityStore)
        {
            _entityReplyManager = entityReplyManager;
            _entityReplyStore = entityReplyStore;
            _entityStore = entityStore;
        }

        public async Task<IEntityResult> CreateAsync(EntityReply model)
        {

            _entityReplyManager.Created += async (sender, args) =>
            {
                
                // Get last 5 participants

                var replies = await _entityReplyStore.QueryAsync()
                    .Page(1, 5)
                    .Select<EntityReplyQueryParams>(q =>
                    {
                        q.EntityId.Equals(args.Entity.Id);
                    })
                    .OrderBy("ModifiedDate", OrderBy.Desc)
                    .ToList();

                var postDetails = args.Entity.GetMetaData<PostDetails>() ?? new PostDetails();
                if (replies?.Data != null)
                {
                    var participants = new List<EntityUser>();
                    foreach (var reply in replies.Data)
                    {
                        participants.Add(reply.CreatedBy);
                    }
                    postDetails.Participants = participants;
                }

                args.Entity.SetMetaData(postDetails);

                await _entityStore.UpdateAsync(args.Entity);
                
            };

            return await _entityReplyManager.CreateAsync(model);
        
        }

        public async Task<IEntityResult> UpdateAsync(EntityReply model)
        {

            _entityReplyManager.Updated += (sender, args) =>
            {

            };
            
            return await _entityReplyManager.UpdateAsync(model);
         
        }

        public async Task<IEntityResult> DeleteAsync(int id)
        {

            _entityReplyManager.Updated += (sender, args) =>
            {

            };

            return await _entityReplyManager.DeleteAsync(id);
            
        }


    }
}