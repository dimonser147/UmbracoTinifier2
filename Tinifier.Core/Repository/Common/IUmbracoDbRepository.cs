using NPoco;
using NPoco.fastJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace Tinifier.Core.Repository.Common
{
    public interface IUmbracoDbRepository
    {
        string GetMediaAbsoluteUrl(int id);
        List<int> GetTrashedNodes();
        List<UmbracoNode> GetNodesByName(string name);
    }


    public class UmbracoDbRepository : IUmbracoDbRepository
    {
        IScopeProvider _scopeProvider;

        public UmbracoDbRepository(IScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public string GetMediaAbsoluteUrl(int id)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                IUmbracoDatabase database = scope.Database;
                var query = new Sql("SELECT Data FROM CmsContentNu WHERE NodeId = @0", id);
                var serializedRootObject = database.FirstOrDefault<string>(query);
                if (serializedRootObject != null)
                {
                    RootObject rootObject = JSON.ToObject<RootObject>(serializedRootObject);
                    var umbracoFile = rootObject.properties.umbracoFile.FirstOrDefault();
                    if (umbracoFile != null)
                    {
                        Val valModel = JSON.ToObject<Val>(umbracoFile.val);
                        return valModel.src;
                    }
                }
            }
            return "";
        }

        public List<int> GetTrashedNodes()
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                IUmbracoDatabase database = scope.Database;
                var query = new Sql("SELECT Id FROM UmbracoNode WHERE trashed = 1");
                var trashedIds = database.Fetch<int>(query);
                return trashedIds;
            }
        }

        public List<UmbracoNode> GetNodesByName(string name)
        {
            using (IScope scope = _scopeProvider.CreateScope())
            {
                IUmbracoDatabase database = scope.Database;
                var query = new Sql("SELECT Id FROM UmbracoNode WHERE text = @0", name);
                return database.Fetch<UmbracoNode>(query);
            }

        }

    }


    public class UmbracoNode
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public int ParentId { get; set; }
        public int Level { get; set; }
        public string Path { get; set; }
        public int SortOrder { get; set; }
        public bool Trashed { get; set; }
        public string Text { get; set; }
    }

    public class UmbracoFile
    {
        public string culture { get; set; }
        public string seg { get; set; }
        public string val { get; set; }
    }

    public class Val
    {
        public string src { get; set; }
        public object crops { get; set; }
    }

    public class UmbracoWidth
    {
        public string culture { get; set; }
        public string seg { get; set; }
        public int val { get; set; }
    }



    public class UmbracoHeight
    {
        public string culture { get; set; }
        public string seg { get; set; }
        public int val { get; set; }
    }

    public class UmbracoByte
    {
        public string culture { get; set; }
        public string seg { get; set; }
        public string val { get; set; }
    }

    public class UmbracoExtension
    {
        public string culture { get; set; }
        public string seg { get; set; }
        public string val { get; set; }
    }

    public class Properties
    {
        public List<UmbracoFile> umbracoFile { get; set; }
        public List<UmbracoWidth> umbracoWidth { get; set; }
        public List<UmbracoHeight> umbracoHeight { get; set; }
        public List<UmbracoByte> umbracoBytes { get; set; }
        public List<UmbracoExtension> umbracoExtension { get; set; }
    }

    public class CultureData
    {
    }

    public class RootObject
    {
        public Properties properties { get; set; }
        public CultureData cultureData { get; set; }
        public string urlSegment { get; set; }
    }
}
