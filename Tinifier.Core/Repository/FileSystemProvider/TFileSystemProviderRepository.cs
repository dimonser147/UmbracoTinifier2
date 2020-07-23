using NPoco;
using Tinifier.Core.Models.Db;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.FileSystemProvider
{
    public class TFileSystemProviderRepository : IFileSystemProviderRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public TFileSystemProviderRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public void Create(string type)
        {
            var settings = new TFileSystemProviderSettings
            {
                Type = type
            };

            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert(settings);
                scope.Complete();
            }
        }

        public TFileSystemProviderSettings GetFileSystem()
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT * FROM TinifierFileSystemProviderSettings");
                return database.FirstOrDefault<TFileSystemProviderSettings>(query);
            }
        }

        public void Delete()
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("DELETE FROM TinifierFileSystemProviderSettings");
                database.Execute(query);
                scope.Complete();
            }
        }
    }
}
