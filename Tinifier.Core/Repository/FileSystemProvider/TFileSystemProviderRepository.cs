using System;
using NPoco;
using Tinifier.Core.Models.Db;
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

            try
            {
                using (IScope scope = _scopeProvider.CreateScope())
                {
                    var database = scope.Database;
                    database.Insert(settings);
                    scope.Complete();
                }
            }
            catch
            {
                //Assume table doesn't exist yet
            }
        }

        public TFileSystemProviderSettings GetFileSystem()
        {
            try
            {
                using (IScope scope = _scopeProvider.CreateScope())
                {
                    var database = scope.Database;
                    var query = new Sql("SELECT * FROM TinifierFileSystemProviderSettings");
                    return database.FirstOrDefault<TFileSystemProviderSettings>(query);
                }
            }
            catch (Exception e)
            {
                return new TFileSystemProviderSettings
                {
                    Type = "PhysicalFileSystem"
                };
            }
        }

        public void Delete()
        {
            try
            {
                using (IScope scope = _scopeProvider.CreateScope())
                {
                    var database = scope.Database;
                    var query = new Sql("DELETE FROM TinifierFileSystemProviderSettings");
                    database.Execute(query);
                    scope.Complete();
                }
            }
            catch 
            { 
                //Assume table doesn't exist yet
            }
        }
    }
}
