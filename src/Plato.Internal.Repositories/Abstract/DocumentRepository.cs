﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Models;
using Plato.Internal.Models.Abstract;

namespace Plato.Internal.Repositories.Abstract
{
    public class DocumentRepository : IDocumentRepository
    {

        #region Private Variables"

        private readonly IDbContext _dbContext;
        private readonly ILogger<DictionaryRepository> _logger;

        #endregion

        #region "Constructor"

        public DocumentRepository(
            IDbContext dbContext,
            ILogger<DictionaryRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }


        #endregion

        #region "Implementation"

        public async Task<DocumentEntry> UpdateAsync(DocumentEntry document)
        {
            var id = await InsertUpdateInternal(
                document.Id,
                document.Value,
                document.CreatedDate,
                document.CreatedUserId,
                document.ModifiedDate,
                document.ModifiedUserId);
            if (id > 0)
                return await GetAsync(id);
            return null;
        }

        public async Task<DocumentEntry> GetAsync(int id)
        {
            return await SelectByIdAsync(id);
        }

        public Task<bool> DeleteAsync(DocumentEntry document)
        {
            throw new NotImplementedException();
        }
        
        #endregion

        #region "Private Methods"

        async Task<int> InsertUpdateInternal(
            int id,
            string value,
            DateTime? createdDate,
            int createdUserId,
            DateTime? modifiedDate,
            int modifiedUserId
        )
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(id == 0
                    ? $"Inserting document with value: {value}"
                    : $"Updating document with id: {id}");
            }

            using (var context = _dbContext)
            {
                if (context == null)
                    return 0;
                return await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateDocument",
                    id,
                    value.ToEmptyIfNull(),
                    createdDate.ToDateIfNull(),
                    createdUserId,
                    modifiedDate.ToDateIfNull(),
                    modifiedUserId);
            }

        }

        async Task<DocumentEntry> SelectByIdAsync(int id)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"Selecting document entry {id}");

            DocumentEntry entry = null;
            // database context may not be configured.
            // For example during set-up
            if (_dbContext != null)
            {
                using (var context = _dbContext)
                {
                    var reader = await context.ExecuteReaderAsync(
                        CommandType.StoredProcedure,
                        "SelectDocumentById",
                        id);
                    if (reader != null)
                    {
                        if (reader.HasRows)
                        {
                            entry = new DocumentEntry();
                            entry.PopulateModel(reader);
                        }
                    }
                }
            }

            return entry;
        }


        #endregion








    }

}