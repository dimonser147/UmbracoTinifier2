using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Infrastructure.Exceptions;
using Tinifier.Core.Models.API;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Image;
using Tinifier.Core.Services.Validation;
using Umbraco.Core.IO;
using uModels = Umbraco.Core.Models;
using uMedia = Umbraco.Core.Models.Media;
using Tinifier.Core.Services.TinyPNG;
using Tinifier.Core.Services.Settings;
using System.Web;
using Tinifier.Core.Services.History;
using Tinifier.Core.Services.Statistic;
using Tinifier.Core.Services.State;
using Umbraco.Core.Services;
using System.Web.Hosting;
using System.Drawing;
using Microsoft.Azure.Storage.Blob;
using Our.Umbraco.FileSystemProviders.Azure;
using Tinifier.Core.Repository.History;
using Tinifier.Core.Models;
using File = System.IO.File;
using Tinifier.Core.Repository.Common;
using Tinifier.Core.Repository.FileSystemProvider;
using Tinifier.Core.Services.FileSystem;

namespace Tinifier.Core.Services.Media
{
    public class TImageService : IImageService
    {
        private readonly IValidationService _validationService;
        private readonly IImageRepository _imageRepository;
        private readonly ITinyImageService _tinyImageService;
        private readonly ISettingsService _settingsService;
        private readonly IHistoryService _historyService;
        private readonly IStatisticService _statisticService;
        private readonly IStateService _stateService;
        private readonly IImageHistoryService _imageHistoryService;
        private readonly IMediaService _mediaService;
        private readonly ITinyPNGConnector _tinyPngConnectorService;
        private readonly IMediaHistoryRepository _mediaHistoryRepository;
        private readonly IFileSystem _fileSystem;
        private readonly IUmbracoDbRepository _umbracoDbRepository;
        private readonly IFileSystemRegistrationService _fileSystemRegistrationService;

        public TImageService(IImageRepository imageRepository, IValidationService validationService,
            ITinyImageService tinyImageService, ISettingsService settingsService, IHistoryService historyService,
            IStatisticService statisticService, IStateService stateService, 
            IImageHistoryService imageHistoryService, IMediaService mediaService, ITinyPNGConnector tinyPngConnectorService,
            IMediaHistoryRepository mediaHistoryRepository, IUmbracoDbRepository umbracoDbRepository, IFileSystemRegistrationService fileSystemRegistrationService)
        {
            _imageRepository = imageRepository;
            _validationService = validationService;
            _tinyImageService = tinyImageService;
            _settingsService = settingsService;
            _historyService = historyService;
            _statisticService = statisticService;
            _stateService = stateService;
            _fileSystemRegistrationService = fileSystemRegistrationService;

            _fileSystem = _fileSystemRegistrationService.GetFileSystem();

           _imageHistoryService = imageHistoryService;
            _mediaService = mediaService;
            _tinyPngConnectorService = tinyPngConnectorService;
            _mediaHistoryRepository = mediaHistoryRepository;
            _umbracoDbRepository = umbracoDbRepository;
            
        }

        public IEnumerable<TImage> Convert(IEnumerable<uModels.Media> items)
        {
            return items
                .Select(x => Convert(x))
                .Where(x => !string.IsNullOrEmpty(x.AbsoluteUrl)); //Skip images for which we were unable to fetch the Url
        }

        public TImage Convert(uModels.Media uMedia)
        {
            TImage image = new TImage()
            {
                Id = uMedia.Id.ToString(),
                Name = uMedia.Name,
                AbsoluteUrl = SolutionExtensions.GetAbsoluteUrl(uMedia)
            };

            if (string.IsNullOrEmpty(image.AbsoluteUrl))
            {
                image.AbsoluteUrl = _umbracoDbRepository.GetMediaAbsoluteUrl(uMedia.Id);
            }

            return image;
        }

        /// <summary>
        /// Update physical media file, method depens on FileSystemProvider
        /// </summary>
        /// <param name="image"></param>
        /// <param name="optimizedImageBytes"></param>
        protected void UpdateMedia(TImage image, byte[] optimizedImageBytes)
        {
            var path = _fileSystem.GetRelativePath(image.AbsoluteUrl);
            if (!_fileSystem.FileExists(path))
                throw new InvalidOperationException("Physical media file doesn't exist in " + path);
            using (Stream stream = new MemoryStream(optimizedImageBytes))
            {
                _fileSystem.AddFile(path, stream, true);
            }
        }

        public IEnumerable<TImage> GetAllImages()
        {
            return Convert(_imageRepository.GetAll());
        }

        public IEnumerable<Umbraco.Core.Models.Media> GetAllImagesAt(int folderId)
        {
            return _imageRepository.GetAllAt(folderId);
        }

        public TImage GetCropImage(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new EntityNotFoundException();

            var fileExt = Path.GetExtension(path).ToUpper().Replace(".", string.Empty).Trim();
            if (!PackageConstants.SupportedExtensions.Contains(fileExt))
                throw new NotSupportedExtensionException(fileExt);

            var tImage = new TImage
            {
                Id = path,
                Name = Path.GetFileName(path),
                AbsoluteUrl = path
            };

            return tImage;
        }

        public IEnumerable<TImage> GetFolderImages(int folderId)
        {
            return Convert(_imageRepository.GetItemsFromFolder(folderId));
        }

        public TImage GetImage(int id)
        {
            return GetImage(_imageRepository.Get(id));
        }

        public TImage GetImage(string path)
        {
            return GetImage(_imageRepository.Get(path));
        }

        public IEnumerable<TImage> GetOptimizedImages()
        {
            return Convert(_imageRepository.GetOptimizedItems().OrderByDescending(x => x.UpdateDate));
        }

        public IEnumerable<TImage> GetTopOptimizedImages()
        {
            return _imageRepository.GetTopOptimizedImages();
        }

        public void Move(Umbraco.Core.Models.Media image, int parentId)
        {
            _imageRepository.Move(image, parentId);
        }

        public async Task OptimizeImageAsync(TImage image)
        {

            _stateService.CreateState(1);
            var tinyResponse = await _tinyPngConnectorService.TinifyAsync(image, _fileSystem).ConfigureAwait(false);
            if (tinyResponse.Output.Url == null)
            {
                _historyService.CreateResponseHistory(image.Id, tinyResponse);
                return;
            }
            UpdateImageAfterSuccessfullRequest(tinyResponse, image, _fileSystem);
            // SendStatistic();
        }

        private void SendStatistic()
        {
            try
            {
                //    var userDomain = HttpContext.Current.Request.Url.Host;
                //    HostingEnvironment.QueueBackgroundWorkItem(stat => _backendDevsConnectorService.SendStatistic(userDomain));
            }
            catch (Exception)
            {
                return;
            }
        }

        public void UndoTinify(int mediaId)
        {
            byte[] imageBytes;
            var originImage = _imageHistoryService.Get(mediaId);

            if (originImage != null)
            {
                var mediaFile = _mediaService.GetById(mediaId) as uMedia;

                if (File.Exists(originImage.OriginFilePath))
                {
                    using (var file = new FileStream(originImage.OriginFilePath, FileMode.Open))
                        imageBytes = SolutionExtensions.ReadFully(file);

                    if (Directory.Exists(Path.GetDirectoryName(originImage.OriginFilePath)))
                        File.Delete(originImage.OriginFilePath);

                    var image = new TImage
                    {
                        Id = mediaId.ToString(),
                        Name = mediaFile.Name,
                        AbsoluteUrl = SolutionExtensions.GetAbsoluteUrl(mediaFile)
                    };

                    // update physical file
                    UpdateMedia(image, imageBytes);
                    // update umbraco media attributes
                    _imageRepository.Update(mediaId, imageBytes.Length);
                    _historyService.Delete(mediaId.ToString());
                    // update statistic
                    _statisticService.UpdateStatistic();
                    //delete image history
                    _imageHistoryService.Delete(mediaId);
                }
            }
            else
            {
                throw new UndoTinifierException("Image not optimized or Undo tinify not enabled");
            }
        }

        public void UpdateImageAfterSuccessfullRequest(TinyResponse tinyResponse, TImage image, IFileSystem fs)
        {
            int.TryParse(image.Id, out var id);

            // download optimized image
            var tImageBytes = _tinyImageService.DownloadImage(tinyResponse.Output.Url);

            // preserve image metadata
            if (_settingsService.GetSettings().PreserveMetadata)
            {
                var originImageBytes = image.ToBytes(fs);
                PreserveImageMetadata(originImageBytes, ref tImageBytes);
            }

            // httpContext is null when optimization on upload
            // https://our.umbraco.org/projects/backoffice-extensions/tinifier/bugs/90472-error-systemargumentnullexception-value-cannot-be-null
            if (HttpContext.Current == null)
            {
                HttpContext.Current = new HttpContext(new SimpleWorkerRequest("dummy.aspx", "", new StringWriter()));
            }

            // update physical file
            UpdateMedia(image, tImageBytes);
            // update history
            _historyService.CreateResponseHistory(image.Id, tinyResponse);
            // update umbraco media attributes
            _imageRepository.Update(id, tinyResponse.Output.Size);
            // update statistic
            _statisticService.UpdateStatistic();
            // update tinifying state
            _stateService.UpdateState();
        }

        private TImage GetImage(uMedia uMedia)
        {
            _validationService.ValidateExtension(uMedia);
            return Convert(uMedia);
        }

        protected void PreserveImageMetadata(byte[] originImage, ref byte[] optimizedImage)
        {
            var originImg = (Image)new ImageConverter().ConvertFrom(originImage);
            var optimisedImg = (Image)new ImageConverter().ConvertFrom(optimizedImage);
            var srcPropertyItems = originImg.PropertyItems;
            foreach (var item in srcPropertyItems)
            {
                optimisedImg.SetPropertyItem(item);
            }

            using (var ms = new MemoryStream())
            {
                optimisedImg.Save(ms, optimisedImg.RawFormat);
                optimizedImage = ms.ToArray();
            }
        }

        public void BackupMediaPaths(IEnumerable<uMedia> media)
        {
            foreach (var m in media)
            {
                var mediaHistory = new TinifierMediaHistory
                {
                    MediaId = m.Id,
                    FormerPath = m.Path,
                    OrganizationRootFolderId = m.ParentId
                };
                _mediaHistoryRepository.Create(mediaHistory);
            }
        }

        public void DiscardOrganizing(int folderId)
        {
            if (!IsFolderOrganized(folderId))
                throw new OrganizationConstraintsException("This folder is not an organization root.");

            Discard(folderId);
        }

        public bool IsFolderChildOfOrganizedFolder(int sourceFolderId)
        {
            var organizedFoldersList = _mediaHistoryRepository.GetOrganazedFolders();

            while (sourceFolderId != -1 && organizedFoldersList.Any())
            {
                var isOrganized = organizedFoldersList.Contains(sourceFolderId);

                if (isOrganized)
                    return true;

                sourceFolderId = _mediaService.GetById(sourceFolderId).ParentId;
            }

            return organizedFoldersList.Contains(-1);
        }

        private void Discard(int baseFolderId)
        {
            var media = _mediaHistoryRepository.GetAll().Where(m => m.OrganizationRootFolderId == baseFolderId);

            foreach (var m in media)
            {
                var monthFolder = _mediaService.GetParent(m.MediaId);
                if (monthFolder == null)
                    continue;
                var yearFolder = _mediaService.GetParent(monthFolder);
                if (yearFolder == null)
                    continue;

                var folderId = yearFolder.Id;
                // the path is stored as a string with comma separated IDs of media
                // where the last value is ID of current media, penultimate value is ID of its root, etc.
                // the first value is ID of the very root media
                var path = m.FormerPath.Split(',');
                var formerParentId = int.Parse(path[path.Length - 2]);
                var image = _imageRepository.Get(m.MediaId);
                Move(image, formerParentId);

                if (!_mediaService.HasChildren(monthFolder.Id))
                {
                    _mediaService.Delete(_mediaService.GetById(monthFolder.Id));
                }

                if (!_mediaService.HasChildren(yearFolder.Id))
                {
                    _mediaService.Delete(_mediaService.GetById(folderId));
                }
            }

            _mediaHistoryRepository.DeleteAll(baseFolderId);
        }

        private bool IsFolderOrganized(int folderId)
        {
            var organizedFoldersList = _mediaHistoryRepository.GetOrganazedFolders();
            return organizedFoldersList.Contains(folderId);
        }

    }
}
