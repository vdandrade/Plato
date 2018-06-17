﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Plato.Internal.Layout.Views;

namespace Plato.Internal.Layout
{

    public interface ILayoutUpdater
    {

        Task<ILayoutUpdater> GetLayoutAsync(Controller controller);

        Task<ILayoutUpdater> UpdateLayoutAsync(
            Func<LayoutViewModel, Task<LayoutViewModel>> configure);

        Task<List<IPositionedView>> UpdateZoneAsync(
            IEnumerable<IPositionedView> zone,
            Action<List<IPositionedView>> configure);
        
        Task<List<IPositionedView>> GetZoneAsync(string zoneName);

    }

    public class LayoutUpdater : ILayoutUpdater
    {

        private LayoutViewModel _model;
        private Controller _controller;

        private readonly ILogger<LayoutUpdater> _logger;
     
        public LayoutUpdater(
            ILogger<LayoutUpdater> logger)
        {
            _logger = logger;
        }
        
        public Task<ILayoutUpdater> GetLayoutAsync(Controller controller)
        {

            _controller = controller;

            // Ensure our controller implements LayoutViewModel
            var model = _controller.ViewData.Model as LayoutViewModel;
            _model = model ?? null;

            return Task.FromResult((ILayoutUpdater)this);

        }

        public async Task<ILayoutUpdater> UpdateLayoutAsync(
            Func<LayoutViewModel, Task<LayoutViewModel>> configure)
        {
            
            // We always need a model to invoke configuration
            if (_model == null)
            {
                return this;
            }

            // Configure model
            if (configure != null)
            {
                _model = await configure.Invoke(_model);
            }
         
            // Attempt to update controllers model to reflect configuration
            try
            {
                await _controller.TryUpdateModelAsync(_model);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
            
            return this;
            
        }
        
        public Task<List<IPositionedView>> GetZoneAsync(string zoneName)
        {

            // We always need a model to invoke configuration
            if (_model == null)
            {
                return null;
            }

            // Use reflection to get models property value
            var propValue = _model
                .GetType()
                .GetProperty(zoneName)
                ?.GetValue(_model, null);

            // Ensure we are working with a list of positioned views
            if (propValue is IEnumerable<IPositionedView> views)
            {
                return Task.FromResult(views.ToList());
            }

            return Task.FromResult(new List<IPositionedView>());

        }

        public Task<List<IPositionedView>> UpdateZoneAsync(
            IEnumerable<IPositionedView> zone,
            Action<List<IPositionedView>> configure)
        {

            if (zone == null)
            {
                zone = new List<IPositionedView>();
            }

            // Convert existing views to list
            var list = zone.ToList();

            // Configure our zone
            configure(list);

            // Order views in zone
            var orderedViews = list.ToList().OrderBy(v => v.Position.Order);

            // Return the configured zone
            return Task.FromResult(orderedViews.ToList());

        }
        
    }

}