﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Plato.Discuss.Models;
using Plato.Entities.Services;
using Plato.Entities.ViewModels;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Layout.ViewProviders;

namespace Plato.Discuss.ViewProviders
{
    public class TopicViewProvider : BaseViewProvider<Topic>
    {

        public const string EditorHtmlName = "message";

        private readonly IEntityViewIncrementer<Topic> _viewIncrementer;
    
        private readonly HttpRequest _request;
        
        public TopicViewProvider(
            IEntityViewIncrementer<Topic> viewIncrementer,
            IHttpContextAccessor httpContextAccessor)
        {
            _request = httpContextAccessor.HttpContext.Request;
            _viewIncrementer = viewIncrementer;
        }

        public override Task<IViewProviderResult> BuildIndexAsync(Topic topic, IViewProviderContext context)
        {

            var viewModel = context.Controller.HttpContext.Items[typeof(EntityIndexViewModel<Topic>)] as EntityIndexViewModel<Topic>;
            if (viewModel == null)
            {
                throw new Exception($"A view model of type {typeof(EntityIndexViewModel<Topic>).ToString()} has not been registered on the HttpContext!");
            }

            return Task.FromResult(Views(
                View<EntityIndexViewModel<Topic>>("Home.Index.Header", model => viewModel).Zone("header"),
                View<EntityIndexViewModel<Topic>>("Home.Index.Tools", model => viewModel).Zone("tools"),
                View<EntityIndexViewModel<Topic>>("Home.Index.Content", model => viewModel).Zone("content")
            ));

        }
        
        public override async Task<IViewProviderResult> BuildDisplayAsync(Topic topic, IViewProviderContext context)
        {
            
            var viewModel = context.Controller.HttpContext.Items[typeof(EntityViewModel<Topic, Reply>)] as EntityViewModel<Topic, Reply>;
            if (viewModel == null)
            {
                throw new Exception($"A view model of type {typeof(EntityViewModel<Topic, Reply>).ToString()} has not been registered on the HttpContext!");
            }

            // Increment entity views
            await _viewIncrementer
                .Contextulize(context.Controller.HttpContext)
                .IncrementAsync(topic);

            return Views(
                View<Topic>("Home.Display.Header", model => topic).Zone("header"),
                View<Topic>("Home.Display.Tools", model => topic).Zone("tools"),
                View<Topic>("Home.Display.Sidebar", model => topic).Zone("sidebar").Order(int.MinValue + 10),
                View<EntityViewModel<Topic, Reply>>("Home.Display.Content", model => viewModel).Zone("content"),
                View<EditEntityReplyViewModel>("Home.Display.Footer", model => new EditEntityReplyViewModel()
                {
                    EntityId = topic.Id,
                    EditorHtmlName = EditorHtmlName
                }).Zone("footer").Order(int.MinValue),
                View<EntityViewModel<Topic, Reply>>("Home.Display.Actions", model => viewModel)
                    .Zone("actions")
                    .Order(int.MaxValue)
            );

        }
        
        public override Task<IViewProviderResult> BuildEditAsync(Topic topic, IViewProviderContext updater)
        {

            // Ensures we persist the message between post backs
            var message = topic.Message;
            if (_request.Method == "POST")
            {
                foreach (string key in _request.Form.Keys)
                {
                    if (key == EditorHtmlName)
                    {
                        message = _request.Form[key];
                    }
                }
            }
          
            var viewModel = new EditEntityViewModel()
            {
                Id = topic.Id,
                Title = topic.Title,
                Message = message,
                EditorHtmlName = EditorHtmlName,
                Alias = topic.Alias
            };
     
            return Task.FromResult(Views(
                View<EditEntityViewModel>("Home.Edit.Header", model => viewModel).Zone("header"),
                View<EditEntityViewModel>("Home.Edit.Content", model => viewModel).Zone("content"),
                View<EditEntityViewModel>("Home.Edit.Footer", model => viewModel).Zone("Footer")
            ));

        }
        
        public override async Task<bool> ValidateModelAsync(Topic topic, IUpdateModel updater)
        {
            return await updater.TryUpdateModelAsync(new EditEntityViewModel
            {
                Title = topic.Title,
                Message = topic.Message
            });
        }

        public override async Task ComposeModelAsync(Topic topic, IUpdateModel updater)
        {

            var model = new EditEntityViewModel
            {
                Title = topic.Title,
                Message = topic.Message
            };

            await updater.TryUpdateModelAsync(model);

            if (updater.ModelState.IsValid)
            {

                topic.Title = model.Title;
                topic.Message = model.Message;
            }

        }
        
        public override async Task<IViewProviderResult> BuildUpdateAsync(Topic topic, IViewProviderContext context)
        {
            return await BuildEditAsync(topic, context);
        }
        
    }

}
