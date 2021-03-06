﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Internal.Cache.Abstractions;
using Plato.Internal.Data.Abstractions;
using Plato.Labels.Models;
using Plato.Labels.Repositories;

namespace Plato.Labels.Stores
{

    public class EntityLabelStore : IEntityLabelStore<EntityLabel>
    {

        private const string ByEntityId = "ByEntityId";

        private readonly IEntityLabelRepository<EntityLabel> _entityLabelRepository;
        private readonly ILogger<EntityLabelStore> _logger;
        private readonly IDbQueryConfiguration _dbQuery;
        private readonly ICacheManager _cacheManager;

        public EntityLabelStore(
            IEntityLabelRepository<EntityLabel> entityLabelRepository,
            ILogger<EntityLabelStore> logger,
            IDbQueryConfiguration dbQuery,
            ICacheManager cacheManager)
        {
            _entityLabelRepository = entityLabelRepository;
            _cacheManager = cacheManager;
            _logger = logger;
            _dbQuery = dbQuery;
        }
        
        #region "Implementation"

        public async Task<EntityLabel> CreateAsync(EntityLabel model)
        {

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Id > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.Id));
            }

            if (model.LabelId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.LabelId));
            }
            
            if (model.EntityId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.EntityId));
            }
            
            var result = await _entityLabelRepository.InsertUpdateAsync(model);
            if (result != null)
            {
                CancelTokens(result);
            }

            return result;
        }

        public async Task<EntityLabel> UpdateAsync(EntityLabel model)
        {

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.Id));
            }

            if (model.LabelId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.LabelId));
            }

            if (model.EntityId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(model.EntityId));
            }

            var result = await _entityLabelRepository.InsertUpdateAsync(model);
            if (result != null)
            {
                CancelTokens(result);
            }

            return result;
        }

        public async Task<bool> DeleteAsync(EntityLabel model)
        {
            var success = await _entityLabelRepository.DeleteAsync(model.Id);
            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Deleted Label role for Label '{0}' with id {1}",
                        model.LabelId, model.Id);
                }

                CancelTokens(model);
            }

            return success;
        }

        public async Task<EntityLabel> GetByIdAsync(int id)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), id);
            return await _cacheManager.GetOrCreateAsync(token,
                async (cacheEntry) => await _entityLabelRepository.SelectByIdAsync(id));
        }

        public IQuery<EntityLabel> QueryAsync()
        {
            var query = new EntityLabelQuery(this);
            return _dbQuery.ConfigureQuery<EntityLabel>(query); ;
        }

        public async Task<IPagedResults<EntityLabel>> SelectAsync(IDbDataParameter[] dbParams)
        {
            var token = _cacheManager.GetOrCreateToken(this.GetType(), dbParams.Select(p => p.Value).ToArray());
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) =>
            {

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Selecting entity labels for key '{0}' with the following parameters: {1}",
                        token.ToString(), dbParams.Select(p => p.Value));
                }

                return await _entityLabelRepository.SelectAsync(dbParams);

            });
        }

        public async Task<IEnumerable<EntityLabel>> GetByEntityIdAsync(int entityId)
        {

            if (entityId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entityId));
            }

            var token = _cacheManager.GetOrCreateToken(this.GetType(), ByEntityId, entityId);
            return await _cacheManager.GetOrCreateAsync(token, async (cacheEntry) =>
            {

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Selecting entity labels for entityId '{0}'.",
                        entityId);
                }

                return await _entityLabelRepository.SelectByEntityIdAsync(entityId);

            });
        }

        public async Task<bool> DeleteByEntityIdAsync(int entityId)
        {

            if (entityId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entityId));
            }

            var success = await _entityLabelRepository.DeleteByEntityIdAsync(entityId);
            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Deleted all labels for entityId '{0}'",
                        entityId);
                }

                CancelTokens();
            }

            return success;
        }

        public async Task<bool> DeleteByEntityIdAndLabelIdAsync(int entityId, int labelId)
        {

            var success = await _entityLabelRepository.DeleteByEntityIdAndLabelIdAsync(entityId, labelId);
            if (success)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Deleted entity label for entityId '{0}' and labelId {1}",
                        entityId, labelId);
                }

                CancelTokens();

            }

            return success;
        }

        public void CancelTokens(EntityLabel model = null)
        {
            _cacheManager.CancelTokens(this.GetType());
        }

        #endregion

    }
}
