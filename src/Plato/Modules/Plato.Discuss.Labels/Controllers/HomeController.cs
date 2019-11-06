﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Navigation.Abstractions;
using Plato.Labels.Stores;
using Plato.Discuss.Models;
using Plato.Discuss.Labels.Models;
using Plato.Labels.ViewModels;
using Plato.Entities.ViewModels;
using Plato.Internal.Layout;
using Plato.Internal.Layout.Titles;
using Microsoft.AspNetCore.Routing;

namespace Plato.Discuss.Labels.Controllers
{
    public class HomeController : Controller, IUpdateModel
    {

        private readonly IViewProviderManager<Label> _labelViewProvider;
        private readonly ILabelStore<Label> _labelStore;
        private readonly IBreadCrumbManager _breadCrumbManager;
        private readonly IPageTitleBuilder _pageTitleBuilder;
        private readonly IContextFacade _contextFacade;
        private readonly IFeatureFacade _featureFacade;

        public IHtmlLocalizer T { get; }

        public IStringLocalizer S { get; }
        
        public HomeController(
            IHtmlLocalizer htmlLocalizer,
            IStringLocalizer stringLocalizer,
            IViewProviderManager<Label> labelViewProvider,
            IBreadCrumbManager breadCrumbManager,
            IPageTitleBuilder pageTitleBuilder,
            ILabelStore<Label> labelStore,            
            IContextFacade contextFacade,
            IFeatureFacade featureFacade)
        {

            _labelViewProvider = labelViewProvider;
            _breadCrumbManager = breadCrumbManager;
            _pageTitleBuilder = pageTitleBuilder;
            _contextFacade = contextFacade;
            _featureFacade = featureFacade;            
            _labelStore = labelStore;

            T = htmlLocalizer;
            S = stringLocalizer;

        }

        public async Task<IActionResult> Index(LabelIndexOptions opts, PagerOptions pager)
        {

            if (opts == null)
            {
                opts = new LabelIndexOptions();
            }

            if (pager == null)
            {
                pager = new PagerOptions();
            }
            
            // Get default options
            var defaultViewOptions = new LabelIndexOptions();
            var defaultPagerOptions = new PagerOptions();

            // Add non default route data for pagination purposes
            if (opts.Search != defaultViewOptions.Search && !this.RouteData.Values.ContainsKey("opts.search"))
                this.RouteData.Values.Add("opts.search", opts.Search);
            if (opts.Sort != defaultViewOptions.Sort && !this.RouteData.Values.ContainsKey("opts.sort"))
                this.RouteData.Values.Add("opts.sort", opts.Sort);
            if (opts.Order != defaultViewOptions.Order && !this.RouteData.Values.ContainsKey("opts.order"))
                this.RouteData.Values.Add("opts.order", opts.Order);
            if (pager.Page != defaultPagerOptions.Page && !this.RouteData.Values.ContainsKey("pager.page"))
                this.RouteData.Values.Add("pager.page", pager.Page);
            if (pager.Size != defaultPagerOptions.Size && !this.RouteData.Values.ContainsKey("pager.size"))
                this.RouteData.Values.Add("pager.size", pager.Size);
            
            // Build view model
            var viewModel = await GetIndexViewModelAsync(opts, pager);

            // Add view options to context 
            HttpContext.Items[typeof(LabelIndexViewModel<Label>)] = viewModel;
            
            // If we have a pager.page querystring value return paged results
            if (int.TryParse(HttpContext.Request.Query["pager.page"], out var page))
            {
                if (page > 0)
                    return View("GetLabels", viewModel);
            }

            // Return Url for authentication purposes
            ViewData["ReturnUrl"] = _contextFacade.GetRouteUrl(new RouteValueDictionary()
            {
                ["area"] = "Plato.Discuss.Labels",
                ["controller"] = "Home",
                ["action"] = "Index"
            });

            // Breadcrumb
            _breadCrumbManager.Configure(builder =>
            {
                builder.Add(S["Home"], home => home
                    .Action("Index", "Home", "Plato.Core")
                    .LocalNav()
                ).Add(S["Discuss"], discuss => discuss
                    .Action("Index", "Home", "Plato.Discuss")
                    .LocalNav()
                ).Add(S["Labels"]);
            });

            // Return view
            return View((LayoutViewModel) await _labelViewProvider.ProvideIndexAsync(new Label(), this));

        }

        public async Task<IActionResult> Display(EntityIndexOptions opts, PagerOptions pager)
        {

            // Get label
            var label = await _labelStore.GetByIdAsync(opts.LabelId);

            // Ensure label exists
            if (label == null)
            {
                return NotFound();
            }
            
            // Get default options
            var defaultViewOptions = new EntityIndexOptions();
            var defaultPagerOptions = new PagerOptions();

            // Add non default route data for pagination purposes
            if (opts.Search != defaultViewOptions.Search && !this.RouteData.Values.ContainsKey("opts.search"))
                this.RouteData.Values.Add("opts.search", opts.Search);
            if (opts.Sort != defaultViewOptions.Sort && !this.RouteData.Values.ContainsKey("opts.sort"))
                this.RouteData.Values.Add("opts.sort", opts.Sort);
            if (opts.Order != defaultViewOptions.Order && !this.RouteData.Values.ContainsKey("opts.order"))
                this.RouteData.Values.Add("opts.order", opts.Order);
            if (pager.Page != defaultPagerOptions.Page && !this.RouteData.Values.ContainsKey("pager.page"))
                this.RouteData.Values.Add("pager.page", pager.Page);
            if (pager.Size != defaultPagerOptions.Size && !this.RouteData.Values.ContainsKey("pager.size"))
                this.RouteData.Values.Add("pager.size", pager.Size);

            // Build view model
            var viewModel = await GetDisplayViewModelAsync(opts, pager);

            // Add view model to context 
            this.HttpContext.Items[typeof(EntityIndexViewModel<Topic>)] = viewModel;

            // If we have a pager.page querystring value return paged results
            if (int.TryParse(HttpContext.Request.Query["pager.page"], out var page))
            {
                if (page > 0 && !pager.Enabled)
                    return View("GetTopics", viewModel);
            }

            // Return Url for authentication purposes
            ViewData["ReturnUrl"] = _contextFacade.GetRouteUrl(new RouteValueDictionary()
            {
                ["area"] = "Plato.Discuss.Labels",
                ["controller"] = "Home",
                ["action"] = "Display",
                ["opts.labelId"] = label != null ? label.Id.ToString() : "",
                ["opts.alias"] = label != null ? label.Alias.ToString() : ""
            });

            // Build page title
            _pageTitleBuilder.AddSegment(S[label.Name], int.MaxValue);

            // Build breadcrumb
            _breadCrumbManager.Configure(builder =>
            {
                builder.Add(S["Home"], home => home
                    .Action("Index", "Home", "Plato.Core")
                    .LocalNav()
                ).Add(S["Discuss"], discuss => discuss
                    .Action("Index", "Home", "Plato.Discuss")
                    .LocalNav()
                ).Add(S["Labels"], labels => labels
                    .Action("Index", "Home", "Plato.Discuss.Labels")
                    .LocalNav()
                ).Add(S[label.Name]);
            });
            
            // Return view
            return View((LayoutViewModel) await _labelViewProvider.ProvideDisplayAsync(label, this));

        }

        // ---------------

        async Task<LabelIndexViewModel<Label>> GetIndexViewModelAsync(LabelIndexOptions options, PagerOptions pager)
        {

            // Get discuss feature
            options.FeatureId = await GetFeatureIdAsync();
       
            if (options.Sort == LabelSortBy.Auto)
            {
                options.Sort = LabelSortBy.Entities;
            }

            // Set pager call back Url
            pager.Url = _contextFacade.GetRouteUrl(pager.Route(RouteData));

            return new LabelIndexViewModel<Label>()
            {
                Options = options,
                Pager = pager
            };

        }

        async Task<EntityIndexViewModel<Topic>> GetDisplayViewModelAsync(EntityIndexOptions options, PagerOptions pager)
        {

            // Get discuss feature
            options.FeatureId = await GetFeatureIdAsync();
            
            // Set pager call back Url
            pager.Url = _contextFacade.GetRouteUrl(pager.Route(RouteData));

            // Return updated model
            return new EntityIndexViewModel<Topic>()
            {
                Options = options,
                Pager = pager
            };

        }

        async Task<int> GetFeatureIdAsync()
        {
            var feature = await _featureFacade.GetFeatureByIdAsync("Plato.Discuss");
            if (feature != null)
            {
                return feature.Id;
            }

            throw new Exception($"Could not find required feature registration for Plato.Discuss");
        }


    }

}
