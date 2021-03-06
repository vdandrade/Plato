﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Plato.Internal.Models.Shell;
using Plato.Internal.Hosting.Abstractions;
using Plato.Discuss.Share.Navigation;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Security.Abstractions;
using Plato.Discuss.Share.Handlers;
using Plato.Internal.Navigation.Abstractions;

namespace Plato.Discuss.Share
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

            // Navigation providers
            services.AddScoped<INavigationProvider, TopicMenu>();
            services.AddScoped<INavigationProvider, TopicReplyMenu>();

            // Register permissions provider
            services.AddScoped<IPermissionsProvider<Permission>, Permissions>();
            
        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {

            routes.MapAreaRoute(
                name: "DiscussShare",
                areaName: "Plato.Discuss.Share",
                template: "discuss/t/share/{opts.id}/{opts.alias}/{opts.replyId?}",
                defaults: new { controller = "Home", action = "Index" }
            );
            

        }
    }
}