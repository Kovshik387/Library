using System;
using System.Collections.Generic;
using Library.Exceptions;
using Library.Interfaces;
using Library.Repositories.Enums;
using Library.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Library.Services
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IEnumerable<IDictionaryRepository> _repository;
        private readonly DataBaseType _databaseType;
        
        public DictionaryService(IEnumerable<IDictionaryRepository> repository, IConfiguration configuration)
        {
            _repository = repository;
            var type = configuration["DataBaseType"];
            _databaseType = Enum.TryParse<DataBaseType>(type, true, out var dbType)
                ? dbType
                : throw new UnknownDataBaseException(type);
        }

        public void Rebuild()
        {
            var repository = GetRepository(_databaseType);
            
            using (var metaIdx = repository.GetMetaIdxTables())
            {
                repository.RebuildDictionary(metaIdx);
            };
        }

        private IDictionaryRepository GetRepository(DataBaseType databaseType)
        {
            foreach (var repository in _repository)
            {
                if (databaseType == repository.GetProvider())
                {
                    return repository;
                }
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}