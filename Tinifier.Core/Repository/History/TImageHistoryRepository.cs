using NPoco;
using System.Collections.Generic;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Common;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.History
{
    public interface IImageHistoryRepository
    {
        IEnumerable<TinifierImagesHistory> GetAll();

        TinifierImagesHistory Get(int id);

        void Create(TinifierImagesHistory entity);

        void Delete(int id);
    }

    public class TImageHistoryRepository : IImageHistoryRepository
    {
        IScopeProvider _scopeProvider;

        public TImageHistoryRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public IEnumerable<TinifierImagesHistory> GetAll()
        {
            throw new System.NotImplementedException();
        }

        public TinifierImagesHistory Get(int id)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT * FROM TinifierImageHistory WHERE [ImageId] = @0", id.ToString());
                return database.FirstOrDefault<TinifierImagesHistory>(query);
            }
        }

        public void Create(TinifierImagesHistory entity)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert(entity);
                scope.Complete();
            }
        }

        public void Delete(int id)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("DELETE FROM TinifierImageHistory WHERE [ImageId] = @0", id.ToString());
                database.Execute(query);
                scope.Complete();
            }
        }
    }
}
