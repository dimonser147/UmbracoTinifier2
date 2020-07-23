using NPoco;
using System.Collections.Generic;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Common;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.History
{
    public interface IMediaHistoryRepository
    {
        IEnumerable<TinifierMediaHistory> GetAll();

        TinifierMediaHistory Get(int id);

        void Create(TinifierMediaHistory entity);

        void Delete(int Id);

        IEnumerable<int> GetOrganazedFolders();

        void DeleteAll(int baseFolderId);
    }

    public class TMediaHistoryRepository : IMediaHistoryRepository
    {
        private readonly string _tableName = "TinifierMediaHistories";
        IScopeProvider scopeProvider;

        public TMediaHistoryRepository(IScopeProvider scopeProvider)
        {
            this.scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Insert new record into the DB
        /// </summary>
        /// <param name="entity">Model to inserting</param>
        public void Create(TinifierMediaHistory entity)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert(entity);
            }
        }

        /// <summary>
        /// Clear all history
        /// </summary>
        /// <param name="id">Id of a media</param>
        public void Delete(int id)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql($"DELETE FROM {_tableName} WHERE MediaId = {id}");
                database.Execute(query);
            }
        }

        /// <summary>
        /// Clear all history
        /// </summary>
        public void DeleteAll()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql($"DELETE FROM {_tableName}");
                database.Execute(query);
            }
        }

        /// <summary>
        /// Get former state of a certain media
        /// </summary>
        /// <param name="id">Id of a media</param>
        /// <returns>TinifierMediaHistory</returns>
        public TinifierMediaHistory Get(int id)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql($"SELECT MediaId, FormerPath, OrganizationRootFolderId FROM {_tableName} WHERE MediaId = {id}");
                return database.FirstOrDefault<TinifierMediaHistory>(query);
            }
        }

        /// <summary>
        /// Get former state of all media
        /// </summary>
        /// <returns>IEnumerable of TinifierMediaHistory</returns>
        public IEnumerable<TinifierMediaHistory> GetAll()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql($"SELECT MediaId, FormerPath, OrganizationRootFolderId FROM {_tableName}");
                return database.Fetch<TinifierMediaHistory>(query);
            }
        }

        /// <summary>
        /// Gets list of currently optimized folder ids
        /// </summary>
        /// <returns>IEnumerable of int</returns>
        public IEnumerable<int> GetOrganazedFolders()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql($"SELECT DISTINCT OrganizationRootFolderId FROM {_tableName}");
                return database.Fetch<int>(query);
            }
        }

        public void DeleteAll(int baseFolderId)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql($"DELETE FROM {_tableName} WHERE OrganizationRootFolderId = {baseFolderId}");
                database.Execute(query);
            }
        }


    }
}
