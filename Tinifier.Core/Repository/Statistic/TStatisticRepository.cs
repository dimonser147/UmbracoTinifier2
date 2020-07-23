using NPoco;
using System;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Common;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.Statistic
{
    public interface IStatisticRepository
    {
        void Create(TImageStatistic entity);

        TImageStatistic GetStatistic();

        long GetTotalSavedBytes();

        void Update(TImageStatistic entity);
    }

    public class TStatisticRepository : IStatisticRepository
    {
        private readonly IScopeProvider _scopeProvider;

        public TStatisticRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        /// <summary>
        /// Create Statistic
        /// </summary>
        /// <param name="entity">TImageStatistic</param>
        public void Create(TImageStatistic entity)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert(entity);
                scope.Complete();
            }
        }

        /// <summary>
        /// Get Statistic
        /// </summary>
        /// <returns>TImageStatistic</returns>
        public TImageStatistic GetStatistic()
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                try
                {
                    var query = new Sql("SELECT * FROM TinifierImagesStatistic");
                    return database.FirstOrDefault<TImageStatistic>(query);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public long GetTotalSavedBytes()
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("select sum(originsize) - sum(optimizedsize) from [TinifierResponseHistory]");
                return database.ExecuteScalar<long>(query);
            }
        }

        /// <summary>
        /// Update Statistic
        /// </summary>
        /// <param name="entity">TImageStatistic</param>
        public void Update(TImageStatistic entity)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("UPDATE TinifierImagesStatistic SET NumberOfOptimizedImages = @0, TotalNumberOfImages = @1, TotalSavedBytes = @2", entity.NumberOfOptimizedImages, entity.TotalNumberOfImages, entity.TotalSavedBytes);
                database.Execute(query);
                scope.Complete();
            }
        }
    }
}
