using NPoco;
using Tinifier.Core.Models.Db;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.ImageCropperInfo
{
    public class TImageCropperInfoRepository : IImageCropperInfoRepository
    {
        IScopeProvider _scopeProvider;

        public TImageCropperInfoRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public TImageCropperInfo Get(string key)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT * FROM TinifierImageCropperInfo WHERE [Key] = @0", key);
                return database.FirstOrDefault<TImageCropperInfo>(query);
            }
        }

        public void Create(string key, string imageId)
        {
            var info = new TImageCropperInfo
            {
                Key = key,
                ImageId = imageId
            };

            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                database.Insert(info);
                scope.Complete();
            }
        }

        public void Delete(string key)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("DELETE FROM TinifierImageCropperInfo WHERE [Key] = @0", key);
                database.Execute(query);
                scope.Complete();
            }
        }

        public void Update(string key, string imageId)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("UPDATE TinifierImageCropperInfo SET ImageId = @0 WHERE [Key] = @1", imageId, key);
                database.Execute(query);
                scope.Complete();
            }
        }
    }
}
