﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Plato.Entities.Models;
using Plato.Entities.ViewModels;
using Plato.Entities.Stores;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Navigation.Abstractions;

namespace Plato.Entities.Services
{
    
    public class EntityReplyService<TModel> : IEntityReplyService<TModel> where TModel : class, IEntityReply
    {

        private readonly IEntityReplyStore<TModel> _entityReplyStore;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EntityReplyService(
            IEntityReplyStore<TModel> entityReplyStore,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _entityReplyStore = entityReplyStore;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IPagedResults<TModel>> GetRepliesAsync(
            EntityOptions options,
            PagerOptions pager)
        {

            // Get principal
            var principal = _httpContextAccessor.HttpContext.User;


            return await _entityReplyStore.QueryAsync()
                .Take(pager.Page, pager.PageSize)
                .Select<EntityReplyQueryParams>(async q =>
                {
                    q.EntityId.Equals(options.Id);

                    //// Hide private?
                    //if (!await _authorizationService.AuthorizeAsync(principal,
                    //    Permissions.ViewPrivateReplies))
                    //{
                    //    q.HidePrivate.True();
                    //}

                    //// Hide spam?
                    //if (!await _authorizationService.AuthorizeAsync(principal,
                    //    Permissions.ViewSpamReplies))
                    //{
                    //    q.HideSpam.True();
                    //}


                    //// Hide deleted?
                    //if (!await _authorizationService.AuthorizeAsync(principal,
                    //    Permissions.ViewDeletedReplies))
                    //{
                    //    q.HideDeleted.True();
                    //}
                    

                })
                .OrderBy(options.Sort, options.Order)
                .ToList();

        }

    }

}