﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Plato.Discuss.Categories.Models;
using Plato.Discuss.Models;
using Plato.Internal.Models.Shell;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.ViewProviders;
using Plato.Discuss.Categories.Moderators.Navigation;
using Plato.Discuss.Categories.Moderators.ViewProviders;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Security.Abstractions;
using Plato.Moderation.Models;
using Plato.Discuss.Categories.Moderators.Handlers;
using Plato.Discuss.Categories.Moderators.ViewAdapters;
using Plato.Internal.Layout.ViewAdapters;
using Plato.Internal.Navigation.Abstractions;

namespace Plato.Discuss.Categories.Moderators
{

    public class Startup : StartupBase
    {

        private readonly IShellSettings _shellSettings;

        public Startup(IShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public override void ConfigureServices(IServiceCollection services)
        {

            // Feature installation event handler
            services.AddScoped<IFeatureEventHandler, FeatureEventHandler>();

            // Admin view providers
            services.AddScoped<IViewProviderManager<Moderator>, ViewProviderManager<Moderator>>();
            services.AddScoped<IViewProvider<Moderator>, AdminViewProvider>();
         
            // Discuss view providers
            services.AddScoped<IViewProviderManager<Topic>, ViewProviderManager<Topic>>();
            services.AddScoped<IViewProvider<Topic>, TopicViewProvider>();
            
            // Moderation view providers
            services.AddScoped<IViewProviderManager<Moderator>, ViewProviderManager<Moderator>>();
            services.AddScoped<IViewProvider<Moderator>, ModeratorViewProvider>();

            // View Adapters
            services.AddScoped<IViewAdapterProvider, ModerationViewAdapterProvider>();

            // Register permissions providers
            services.AddScoped<IPermissionsProvider<ModeratorPermission>, ModeratorPermissions>();
            services.AddScoped<IPermissionsProvider<Permission>, Permissions>();
            
            // Register navigation providers
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<INavigationProvider, TopicMenu>();
            services.AddScoped<INavigationProvider, TopicReplyMenu>();

        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {

        }

    }
}