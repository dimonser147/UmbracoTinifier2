using NPoco;
using System.Collections.Generic;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Common;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.History
{
    public class THistoryRepository : IHistoryRepository
    {
        IScopeProvider scopeProvider;

        public THistoryRepository(IScopeProvider scopeProvider)
        {
            this.scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Get all tinifing histories from database
        /// </summary>
        /// <returns>IEnumerable of TinyPNGResponseHistory</returns>
        public IEnumerable<TinyPNGResponseHistory> GetAll()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var select = new Sql("SELECT * FROM TinifierResponseHistory WHERE OccuredAt >= DATEADD(day,-10,GETDATE()) AND IsOptimized = 'true'");
                return database.Fetch<TinyPNGResponseHistory>(select);
            }
        }

        /// <summary>
        /// Get history by Id
        /// </summary>
        /// <param name="id">history Id</param>
        /// <returns>TinyPNGResponseHistory</returns>
        public TinyPNGResponseHistory Get(string id)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT * FROM TinifierResponseHistory WHERE ImageId = @0", id);
                return database.FirstOrDefault<TinyPNGResponseHistory>(query);
            }
        }

        public IEnumerable<TinyPNGResponseHistory> GetHistoryByPath(string path)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT * FROM TinifierResponseHistory WHERE ImageId LIKE @0", $"%{path}%");
                return database.Fetch<TinyPNGResponseHistory>(query);
            }
        }

        /// <summary>
        /// Create history
        /// </summary>
        /// <param name="newItem">TinyPNGResponseHistory</param>
        public void Create(TinyPNGResponseHistory newItem)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert(newItem);
                scope.Complete();
            }
        }

        /// <summary>
        /// Delete history for image
        /// </summary>
        /// <param name="imageId">Image Id</param>
        public void Delete(string imageId)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("DELETE FROM TinifierResponseHistory WHERE ImageId = @0", imageId);
                database.Execute(query);
                scope.Complete();
            }
        }
    }
}
