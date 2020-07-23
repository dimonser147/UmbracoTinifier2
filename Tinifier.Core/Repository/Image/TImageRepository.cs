using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Tinifier.Core.Controllers;
using Tinifier.Core.Models.Db;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;


using System.IO;

using System.Web.Hosting;
using Tinifier.Core.Infrastructure;

using Tinifier.Core.Models.Services;
using Tinifier.Core.Repository.Common;
using Umbraco.Core;
using Umbraco.Web;
using NPoco;
using Umbraco.Core.Models.PublishedContent;
using System.Web;
using Tinifier.Core.Repository.FileSystemProvider;

namespace Tinifier.Core.Repository.Image
{
    public class TImageRepository : IImageRepository
    {
        IScopeProvider scopeProvider;
        IMediaTypeService _mediaTypeService;
        IFileSystemProviderRepository _fileSystemProviderRepository;
        private readonly IMediaService _mediaService;

        public TImageRepository(IScopeProvider scopeProvider, IMediaService mediaService,
            IMediaTypeService mediaTypeService, IFileSystemProviderRepository fileSystemProviderRepository)
        {
            this.scopeProvider = scopeProvider;
            this._mediaService = mediaService;
            _mediaTypeService = mediaTypeService;
            _fileSystemProviderRepository = fileSystemProviderRepository;
        }

        /// <summary>
        /// Get all media
        /// </summary>
        /// <returns>IEnumerable of Media</returns>
        public IEnumerable<Media> GetAll()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                IUmbracoDatabase database = scope.Database;
                var query = new Sql("SELECT NodeId FROM UmbracoContentVersion");
                var nodeIds = database.Fetch<int>(query);

                IEnumerable<Media> mediaItems = _mediaService.GetByIds(nodeIds)
                    .Where(media => media.ContentType.Alias.Equals(PackageConstants.ImageAlias))
                    .Select(m => m as Media).ToList();

                return mediaItems;
            }
        }

        /// <summary>
        /// Gets a collection of IMedia objects by ParentId
        /// </summary>
        /// <returns>IEnumerable of Media</returns>
        public IEnumerable<Media> GetAllAt(int id)
        {
            var allImages = GetAll();
            var ourMedia = allImages.Where(media => media.ParentId == id).ToList();
            return ourMedia;
        }

        /// <summary>
        /// Get Media by Id
        /// </summary>
        /// <param name="id">Media Id</param>
        /// <returns>Media</returns>
        public Media Get(int id)
        {
            return _mediaService.GetById(id) as Media;
        }

        /// <summary>
        /// Get Media By path
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>Media</returns>
        public Media Get(string path)
        {
            return _mediaService.GetMediaByPath(path) as Media;
        }

        /// <summary>
        /// Moves an IMedia object to a new location
        /// </summary>
        /// <param name="media">media to move</param>
        /// <param name="parentId">id of a new location</param>
        public void Move(IMedia media, int parentId)
        {
            _mediaService.Move(media, parentId);
        }

        /// <summary>
        /// Update Media
        /// </summary>
        /// <param name="id">Media Id</param>
        public void Update(int id, int actualSize)
        {
            if (_mediaService.GetById(id) is Media mediaItem)
            {
                mediaItem.SetValue("umbracoBytes", actualSize);
                mediaItem.UpdateDate = DateTime.UtcNow;
                // raiseEvents: false - #2827
                _mediaService.Save(mediaItem, raiseEvents: false);
            }
        }

        /// <summary>
        /// Get Optimized Images
        /// </summary>
        /// <returns>IEnumerable of Media</returns>
        public IEnumerable<Media> GetOptimizedItems()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT ImageId FROM TinifierResponseHistory WHERE IsOptimized = 'true'");
                var historyIds = database.Fetch<string>(query);

                var pardesIds = new List<int>();

                foreach (var historyId in historyIds)
                {
                    if (int.TryParse(historyId, out var parsedId))
                        pardesIds.Add(parsedId);
                }

                var mediaItems = GetAll().
                                 Where(item => pardesIds.Contains(item.Id));

                return mediaItems.Select(item => item as Media).ToList();
            }
        }

        /// <summary>
        /// Get Media from folder
        /// </summary>
        /// <param name="folderId">Folder Id</param>
        /// <returns>IEnumerable of Media</returns>
        public IEnumerable<Media> GetItemsFromFolder(int folderId)
        {
            List<Media> _mediaList = new List<Media>();
            var allImages = GetAll();
            var items = allImages.Where(media => media.ParentId == folderId).ToList();

            if (items.Any())
            {
                foreach (var media in items)
                {
                    if (media.ContentType.Alias == PackageConstants.ImageAlias)
                    {
                        _mediaList.Add(media as Media);
                    }
                }
                foreach (var media in items)
                {
                    if (media.ContentType.Alias == PackageConstants.FolderAlias)
                    {
                        GetItemsFromFolder(media.Id);
                    }
                }
            }
            return _mediaList;
        }

        /// <summary>
        /// Get Count of Images
        /// </summary>
        /// <returns>Number of Images</returns>
        public int AmounthOfItems()
        {
            var numberOfImages = 0;
            var fileSystem = _fileSystemProviderRepository.GetFileSystem();

            if (fileSystem != null)
            {
                // if (fileSystem.Type.Contains("PhysicalFileSystem"))
                // {
                numberOfImages = Directory.EnumerateFiles(HostingEnvironment.MapPath("/media/"), "*.*", SearchOption.AllDirectories)
                    .Count(file => !file.ToLower().EndsWith("config"));
                //}
                // else
                // {
                //    //_blobStorage.SetDataForBlobStorage();
                //    //if (_blobStorage.DoesContainerExist())
                //    //    numberOfImages = _blobStorage.CountBlobsInContainer();
                // }        
            }

            return numberOfImages;
        }

        /// <summary>
        /// Get Count of Optimized Images
        /// </summary>
        /// <returns>Number of optimized Images</returns>
        public int AmounthOfOptimizedItems()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var query = new Sql("SELECT ImageId FROM TinifierResponseHistory WHERE IsOptimized = 'true'");
                var historyIds = database.Fetch<string>(query);
                return historyIds.Count;
            }
        }

        public IEnumerable<TImage> GetTopOptimizedImages()
        {
            using (IScope scope = scopeProvider.CreateScope())
            {
                var database = scope.Database;
                var images = new List<TImage>();
                var query = new Sql("SELECT ImageId, OriginSize, OptimizedSize, OccuredAt FROM TinifierResponseHistory WHERE IsOptimized = 'true'");
                var optimizedImages = database.Fetch<TopImagesModel>(query);
                var historyIds = optimizedImages.OrderByDescending(x => (x.OriginSize - x.OptimizedSize)).Take(50)
                    .OrderByDescending(x => x.OccuredAt).Select(y => y.ImageId);

                var pardesIds = new List<int>();
                var croppedIds = new List<string>();

                foreach (var historyId in historyIds)
                {
                    if (int.TryParse(historyId, out var parsedId))
                        pardesIds.Add(parsedId);
                    else
                        croppedIds.Add(historyId);
                }

                var mediaItems = GetAll()
                    .Where(item => pardesIds.Contains(item.Id));

                foreach (var media in mediaItems)
                {
                    var custMedia = new TImage
                    {
                        Id = media.Id.ToString(),
                        Name = media.Name,
                        AbsoluteUrl = GetAbsoluteUrl(media as Media)
                    };

                    images.Add(custMedia);
                }

                foreach (var crop in croppedIds)
                {
                    var custMedia = new TImage
                    {
                        Id = crop,
                        Name = Path.GetFileName(crop),
                        AbsoluteUrl = crop
                    };

                    images.Add(custMedia);
                }

                return images;
            }
        }

        protected string GetAbsoluteUrl(Media uMedia)
        {
            // var umbHelper = new UmbracoHelper(UmbracoContext.Current);
            // var content = umbHelper.Media(uMedia.Id);
            // var imagerUrl = content.Url;
            //
            //
            // string src = "http://" + HttpContext.Current.Request.Url.Host + uMedia.Path;
            return uMedia.Path;
        }
    }
}
