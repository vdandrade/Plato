﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Plato.Discuss.Labels.ViewModels;
using Plato.Discuss.Models;
using Plato.Entities.Stores;
using Plato.Internal.Hosting.Abstractions;
using Plato.Labels.Models;

namespace Plato.Discuss.Labels.ViewComponents
{
 
    public class LabelListItemViewComponent : ViewComponent
    {

        private readonly IContextFacade _contextFacade;

        public LabelListItemViewComponent(IContextFacade contextFacade, IEntityStore<Topic> entityStore)
        {
            _contextFacade = contextFacade;
        }

        public Task<IViewComponentResult> InvokeAsync(
            LabelListItemViewModel model)
        {
            return Task.FromResult((IViewComponentResult)View(model));
        }

    }

}