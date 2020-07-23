using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Models.Db;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.State
{
    public interface IStateRepository
    {
        IEnumerable<TState> GetAll();

        void Create(TState entity);

        TState Get(int status);

        void Update(TState entity);

        void Delete();

    }

    public class TStateRepository : IStateRepository
    {
        IScopeProvider scopeProvider;

        public TStateRepository(IScopeProvider scopeProvider)
        {
            this.scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Create state
        /// </summary>
        /// <param name="entity">TState</param>
        public void Create(TState entity)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert(entity);
                scope.Complete();
            }
        }

        /// <summary>
        /// Get all states
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TState> GetAll()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT * FROM TinifierState");
                var states = database.Fetch<TState>(query);
                scope.Complete();
                return states;
            }
        }

        /// <summary>
        /// Get state with status
        /// </summary>
        /// <param name="status">status Id</param>
        /// <returns>TState</returns>
        public TState Get(int status)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                try
                {
                    var database = scope.Database;
                    var query = new Sql("SELECT * FROM TinifierState WHERE Status = @0", status);
                    var states = database.FirstOrDefault<TState>(query);
                    scope.Complete();
                    return states;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Update State
        /// </summary>
        /// <param name="entity">TState</param>
        public void Update(TState entity)
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("UPDATE TinifierState SET CurrentImage = @0, AmounthOfImages = @1, Status = @2 WHERE Id = @3", entity.CurrentImage, entity.AmounthOfImages, entity.Status, entity.Id);
                database.Execute(query);
                scope.Complete();
            }
        }

        public void Delete()
        {
            DeleteTinifierTables();
            CreateTinifierTables();
        }

        #region Private
        private void CreateTinifierTables()
        {
            // var logger = LoggerResolver.Current.Logger;
            // var dbContext = ApplicationContext.Current.DatabaseContext;
            // var dbHelper = new DatabaseSchemaHelper(dbContext.Database, logger, dbContext.SqlSyntax);
            //
            // var tables = new Dictionary<string, Type>
            // {
            //     { PackageConstants.DbSettingsTable, typeof(TSetting) },
            //     { PackageConstants.DbHistoryTable, typeof(TinyPNGResponseHistory) },
            //     { PackageConstants.DbStatisticTable, typeof(TImageStatistic) },
            //     { PackageConstants.DbStateTable, typeof(TState) },
            //     { PackageConstants.MediaHistoryTable, typeof(TinifierMediaHistory) }
            // };
            //
            // for (var i = 0; i < tables.Count; i++)
            // {
            //     if (!dbHelper.TableExist(tables.ElementAt(i).Key))
            //     {
            //         dbHelper.CreateTable(false, tables.ElementAt(i).Value);
            //     }
            // }
            //
            // // migrations
            // foreach (var migration in Application.Migrations.MigrationsHelper.GetAllMigrations())
            // {
            //     migration?.Resolve(dbContext);
            // }
        }

        private void DeleteTinifierTables()
        {

            var tables = new Dictionary<string, Type>
             {
                 { PackageConstants.DbSettingsTable, typeof(TSetting) },
                 { PackageConstants.DbHistoryTable, typeof(TinyPNGResponseHistory) },
                 { PackageConstants.DbStatisticTable, typeof(TImageStatistic) },
                 { PackageConstants.DbStateTable, typeof(TState) },
                 { PackageConstants.MediaHistoryTable, typeof(TinifierMediaHistory) }
             };


            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                for (var i = 0; i < tables.Count; i++)
                {
                   // var queryTableExists = new Sql($"SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tables.ElementAt(i).Key}'");
                   // var count = database.FirstOrDefault<TState>(queryTableExists);
                   // if (count.Equals(1))
                   // {
                        var queryDropQuery = new Sql($"DELETE FROM {tables.ElementAt(i).Key}");
                        database.Execute(queryDropQuery);
                        scope.Complete();
                    //}
                }
            }
        }

        #endregion
    }
}
