﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Plato.Categories.Roles.Services;
using Plato.Internal.Features.Abstractions;
using Plato.Questions.Categories.Models;
using Plato.Questions.Categories.Roles.QueryAdapters;
using Plato.Questions.Categories.Roles.ViewProviders;
using Plato.Questions.Models;
using Plato.Internal.Models.Shell;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Stores.Abstractions.QueryAdapters;
using Plato.Questions.Categories.Roles.Handlers;

namespace Plato.Questions.Categories.Roles
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

            // Category role view providers
            services.AddScoped<IViewProviderManager<CategoryAdmin>, ViewProviderManager<CategoryAdmin>>();
            services.AddScoped<IViewProvider<CategoryAdmin>, CategoryRolesViewProvider>();

            // Query adapters to limit access by role
            services.AddScoped<IQueryAdapterProvider<Question>, QuestionQueryAdapter>();
            services.AddScoped<IQueryAdapterProvider<Category>, CategoryQueryAdapter>();

            // Services
            services.AddScoped<IDefaultCategoryRolesManager<Category>, DefaultCategoryRolesManager<Category>>();

        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {
        }
    }
}