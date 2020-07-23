using NPoco;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Common;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.Settings
{
    public class TSettingsRepository : ISettingsRepository
    {
        IScopeProvider _scopeProvider;

        public TSettingsRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Get settings
        /// </summary>
        /// <returns></returns>
        public TSetting GetSettings()
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT * FROM TinifierUserSettings ORDER BY Id DESC");
                return database.FirstOrDefault<TSetting>(query);
            }
        }

        /// <summary>
        /// Create settings
        /// </summary>
        /// <param name="entity">TSetting</param>
        public void Create(TSetting entity)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert(entity);
                scope.Complete();
            }
        }

        /// <summary>
        /// Update currentMonthRequests in settings
        /// </summary>
        /// <param name="currentMonthRequests">currentMonthRequests</param>
        public void Update(int currentMonthRequests)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("UPDATE TinifierUserSettings SET CurrentMonthRequests = @0", currentMonthRequests);
                database.Execute(query);
                scope.Complete();
            }
        }
    }
}
