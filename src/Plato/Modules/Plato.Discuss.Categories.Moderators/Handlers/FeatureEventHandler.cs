﻿using System.Threading.Tasks;
using Plato.Internal.Data.Schemas.Abstractions;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Security.Abstractions;

namespace Plato.Discuss.Categories.Moderators.Handlers
{
    
    public class FeatureEventHandler : BaseFeatureEventHandler
    {
        
        private readonly ISchemaBuilder _schemaBuilder;
        private readonly IDefaultRolesManager _defaultRolesManager;

        public FeatureEventHandler(
            ISchemaBuilder schemaBuilder,
            IDefaultRolesManager defaultRolesManager)
        {
            _schemaBuilder = schemaBuilder;
            _defaultRolesManager = defaultRolesManager;
        }
        
        public override Task InstallingAsync(IFeatureEventContext context)
        {
            return Task.CompletedTask;
        }

        public override async Task InstalledAsync(IFeatureEventContext context)
        {
            // Apply default permissions to default roles for new feature
            await _defaultRolesManager.UpdateDefaultRolesAsync(new Permissions());

        }

        public override Task UninstallingAsync(IFeatureEventContext context)
        {
            return Task.CompletedTask;
        }

        public override Task UninstalledAsync(IFeatureEventContext context)
        {
            return Task.CompletedTask;
        }
        
    }

}
