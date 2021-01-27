﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Infrastructure.Exceptions;
using Tinifier.Core.Models;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Services;
using Tinifier.Core.Services.History;
using Tinifier.Core.Services.Media;
using Tinifier.Core.Services.Settings;
using Tinifier.Core.Services.State;
using Tinifier.Core.Services.TinyPNG;
using Tinifier.Core.Services.Validation;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Web.WebApi;
using Umbraco.Web;
using Newtonsoft.Json.Linq;
using Tinifier.Core.Services.BackendDevs;
using Tinifier.Core.Services.Media.Organizers;
using Umbraco.Core.Models.PublishedContent;
using Tinifier.Core.Services.ImageCropperInfo;
using MenuItemCollection = Umbraco.Web.Models.Trees.MenuItemCollection;
using Tinifier.Core.Filters;
using Tinifier.Core.Services.FileSystem;

namespace Tinifier.Core.Controllers
{
    [ExceptionFilter]
    public class TinifierController : UmbracoAuthorizedApiController
    {
        private readonly IImageService _imageService;
        private readonly ITinyPNGConnector _tinyPngConnectorService;
        private readonly IHistoryService _historyService;
        private readonly ISettingsService _settingsService;
        private readonly IStateService _stateService;
        private readonly IValidationService _validationService;
        private readonly IBackendDevsConnector _backendDevsConnectorService;
        private readonly IFileSystem _fileSystem;
        private readonly IImageCropperInfoService _imageCropperInfoService;
        private readonly IImageOrganizer _imageOrganizer;
        private readonly IFileSystemRegistrationService _fileSystemRegistrationService;

        public TinifierController(IHistoryService historyService,
            ISettingsService settingsService, IValidationService validationService, 
            IStateService stateService, ITinyPNGConnector tinyPngConnectorService, IImageOrganizer imageOrganizer,
             IImageService imageService, IFileSystemRegistrationService fileSystemRegistrationService)
        {
            _historyService = historyService;
            _imageService = imageService;
            _settingsService = settingsService;
            _validationService = validationService;
            _stateService = stateService;
            _tinyPngConnectorService = tinyPngConnectorService;
            _fileSystemRegistrationService = fileSystemRegistrationService;
            _fileSystem = _fileSystemRegistrationService.GetFileSystem();
            _imageOrganizer = imageOrganizer;
        }

        /// <summary>
        /// Get Image by id
        /// </summary>
        /// <param name="timageId">Image Id</param>
        /// <returns>Response(StatusCode, {image, history}}</returns>
        [HttpGet]
        public HttpResponseMessage GetTImage(string timageId)
        {
            TImage timage;

            try
            {
                timage = int.TryParse(timageId, out var imageId)
                    ? _imageService.GetImage(imageId)
                    : _imageService.GetCropImage(SolutionExtensions.Base64Decode(timageId));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest,
                    new TNotification("Tinifier Oops", ex.Message, EventMessageType.Error)
                    {
                        sticky = true,
                    });
            }

            var history = _historyService.GetImageHistory(timage.Id);
            return Request.CreateResponse(HttpStatusCode.OK, new { timage, history });
        }

        [HttpGet]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        /// <summary>
        /// Tinify Image(s)
        /// </summary>
        /// <param name="imageRelativeUrls">Array of media items urls</param>
        /// <param name="mediaId">Media item id</param>
        /// <returns>Response(StatusCode, message)</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> TinyTImage([FromUri]string[] imageRelativeUrls, [FromUri]int mediaId)
        {
            HttpResponseMessage responseMessage;
            _settingsService.CheckIfSettingExists();
            _validationService.ValidateConcurrentOptimizing();

            if (imageRelativeUrls.Length != 0)
            {
                responseMessage = await TinifyImages(imageRelativeUrls);
            }
            else
            {
                if (_validationService.IsFolder(mediaId))
                {
                    responseMessage = await TinifyFolder(mediaId);
                }
                else
                {
                    responseMessage = await TinifyImage(mediaId);
                }
            }

            return responseMessage;
        }

        /// <summary>
        /// Tinify full Media folder
        /// </summary>
        /// <returns>Response(StatusCode, message)</returns>
        [HttpPut]
        public async Task<HttpResponseMessage> TinifyEverything()
        {
            var nonOptimizedImages = new List<TImage>();

            foreach (var image in _imageService.GetAllImages())
            {
                var imageHistory = _historyService.GetImageHistory(image.Id);
                if (imageHistory != null && imageHistory.IsOptimized)
                    continue;
                nonOptimizedImages.Add(image);
            }

            GetAllPublishedContentAndGetImageCroppers(nonOptimizedImages);

            if (nonOptimizedImages.Count == 0)
                return GetImageOptimizedReponse(true);

            _stateService.CreateState(nonOptimizedImages.Count);
            return await CallTinyPngService(nonOptimizedImages);
        }

        /// <summary>
        /// Undo tinify
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<HttpResponseMessage> UndoTinify([FromUri]int mediaId)
        {
            try
            {
                _imageService.UndoTinify(mediaId);
            }
            catch (UndoTinifierException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new TNotification(
                        "Undo finished",
                        ex.Message,
                        EventMessageType.Info)
                );
            }

            return Request.CreateResponse(HttpStatusCode.OK,
                new TNotification("Success", PackageConstants.UndoTinifyingFinished, EventMessageType.Success)
                {
                    sticky = true,
                });
        }

        /// <summary>
        /// Tinify folder By Id
        /// </summary>
        /// <param name="folderId">Folder Id</param>
        /// <returns>Response(StatusCode, message)</returns>
        private async Task<HttpResponseMessage> TinifyFolder(int folderId)
        {
            var images = _imageService.GetFolderImages(folderId);
            var imagesList = _historyService.GetImagesWithoutHistory(images);

            if (imagesList.Count == 0)
                return GetImageOptimizedReponse(true);

            _stateService.CreateState(imagesList.Count);
            return await CallTinyPngService(imagesList);
        }

        /// <summary>
        /// Tinify Images by urls
        /// </summary>
        /// <param name="imagesRelativeUrls">Array of images urls</param>
        /// <returns>Response(StatusCode, message)</returns>
        private async Task<HttpResponseMessage> TinifyImages(IEnumerable<string> imagesRelativeUrls)
        {
            var nonOptimizedImages = new List<TImage>();

            foreach (var imageRelativeUrl in imagesRelativeUrls)
            {
                var image = _imageService.GetImage(imageRelativeUrl);
                var imageHistory = _historyService.GetImageHistory(image.Id);

                if (imageHistory != null && imageHistory.IsOptimized)
                    continue;

                nonOptimizedImages.Add(image);
            }

            if (nonOptimizedImages.Count == 0)
                return GetImageOptimizedReponse(true);

            _stateService.CreateState(nonOptimizedImages.Count);
            return await CallTinyPngService(nonOptimizedImages);
        }

        /// <summary>
        /// Tinify image by Id
        /// </summary>
        /// <param name="imageId">Image Id</param>
        /// <returns>Response(StatusCode, message)</returns>
        private async Task<HttpResponseMessage> TinifyImage(int imageId)
        {
            var imageById = _imageService.GetImage(imageId);

            var notOptimizedImage = _historyService.GetImageHistory(imageById.Id);
            if (notOptimizedImage != null && notOptimizedImage.IsOptimized)
                return GetImageOptimizedReponse();

            var nonOptimizedImages = new List<TImage> { imageById };
            _stateService.CreateState(nonOptimizedImages.Count);
            return await CallTinyPngService(nonOptimizedImages);
        }

        /// <summary>
        /// Create request to TinyPNG service and get response
        /// </summary>
        /// <param name="imagesList">Images that needs to be tinifing</param>
        /// <returns>Response(StatusCode, message)</returns>
        private async Task<HttpResponseMessage> CallTinyPngService(IEnumerable<TImage> imagesList)
        {
            var nonOptimizedImagesCount = 0;
            var userDomain = HttpContext.Current.Request.Url.Host;
            foreach (var tImage in imagesList)
            {
                var tinyResponse = await _tinyPngConnectorService.TinifyAsync(tImage, _fileSystem);

                if (tinyResponse.Output.Url == null)
                {
                    _historyService.CreateResponseHistory(tImage.Id, tinyResponse);
                    _stateService.UpdateState();
                    nonOptimizedImagesCount++;
                    continue;
                }

                _imageService.UpdateImageAfterSuccessfullRequest(tinyResponse, tImage, _fileSystem);

                try
                {
                    //     HostingEnvironment.QueueBackgroundWorkItem(stat => _backendDevsConnectorService.SendStatistic(userDomain));
                }
                catch (NotSuccessfullRequestException)
                {
                }
            }

            var n = imagesList.Count();
            var k = n - nonOptimizedImagesCount;

            if (n > 0)
            {
                _imageService.UpdateStatistics();
            }

            return GetSuccessResponse(k, n,
                nonOptimizedImagesCount == 0 ? EventMessageType.Success : EventMessageType.Warning);
        }

        private HttpResponseMessage GetImageOptimizedReponse(bool isMultipleImages = false)
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                   new TNotification(
                       PackageConstants.TinifyingFinished,
                       isMultipleImages ? PackageConstants.AllImagesAlreadyOptimized : PackageConstants.AlreadyOptimized,
                       EventMessageType.Info)
                       );
        }

        private HttpResponseMessage GetSuccessResponse(int optimized, int total, EventMessageType type)
        {
            return Request.CreateResponse(HttpStatusCode.OK,
                new TNotification(PackageConstants.TinifyingFinished,
                    $"{optimized}/{total} images were optimized. Enjoy the package? Click the <a href=\"https://our.umbraco.org/projects/backoffice-extensions/tinifier/\" target=\"_blank\">message</a> and rate us!", type)
                {
                    url = "https://our.umbraco.org/projects/backoffice-extensions/tinifier/"
                });
        }

        [HttpGet]
        public HttpResponseMessage OrganizeImages(int folderId)
        {
            if (MediaSavingHelper.IsSavingInProgress)
            {
                return Request.CreateResponse(HttpStatusCode.Conflict);
            }
            try
            {
                _imageOrganizer.Organize(folderId);

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (OrganizationConstraintsException c)
            {
                return GetErrorNotification(c.Message, HttpStatusCode.OK, EventMessageType.Warning);
            }
            catch (Exception ex)
            {
                return GetErrorNotification(ex.Message, HttpStatusCode.InternalServerError, EventMessageType.Error);
            }
        }

        [HttpGet]
        public HttpResponseMessage DiscardOrganizing(int folderId)
        {
            try
            {
                _imageService.DiscardOrganizing(folderId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (OrganizationConstraintsException c)
            {
                return GetErrorNotification(c.Message, HttpStatusCode.OK, EventMessageType.Warning);
            }
            catch (Exception ex)
            {
                return GetErrorNotification(ex.Message, HttpStatusCode.InternalServerError, EventMessageType.Error);
            }
        }

        private HttpResponseMessage GetErrorNotification(string message, HttpStatusCode httpStatusCode, EventMessageType eventMessageType)
        {
            return Request.CreateResponse(httpStatusCode,
                    new TNotification("Tinifier Oops", message, eventMessageType) { sticky = true });
        }

        private IEnumerable<IPublishedContent> GetAllPublishedContent()
        {
            // Get all published content
            var allPublishedContent = new List<IPublishedContent>();
            foreach (var publishedContentRoot in Umbraco.ContentAtRoot())
                allPublishedContent.AddRange(publishedContentRoot.DescendantsOrSelf());
            return allPublishedContent;

        }

        private void TinifyImageCroppers(string path, List<TImage> nonOptimizedImages,
            TImageCropperInfo imageCropperInfo, string key)
        {
            var pathForFolder = path.Remove(path.LastIndexOf('/') + 1);
            _imageCropperInfoService.GetFilesAndTinify(pathForFolder, nonOptimizedImages, true);

            if (imageCropperInfo != null)
                _imageCropperInfoService.Update(key, path);
            else
                _imageCropperInfoService.Create(key, path);
        }

        private void GetAllPublishedContentAndGetImageCroppers(List<TImage> nonOptimizedImages)
        {
            foreach (var content in GetAllPublishedContent())
            {
                var imageCroppers = content.Properties
                    .Where(x => !string.IsNullOrEmpty(x.Alias.ToString()) && x.Alias.ToString().Contains("crops"));


                foreach (var crop in imageCroppers)
                {
                    var key = string.Concat(content.Name, "-", crop.Alias);
                    var imageCropperInfo = _imageCropperInfoService.Get(key);
                    var imagePath = crop.Alias;

                    //Wrong object
                    if (imageCropperInfo == null && imagePath == null)
                        continue;

                    //Cropped file was Deleted
                    if (imageCropperInfo != null && imagePath == null)
                    {
                        _imageCropperInfoService.DeleteImageFromImageCropper(key, imageCropperInfo);
                        continue;
                    }

                    //Cropped file was created or updated
                    var json = JObject.Parse(imagePath.ToString());
                    var path = json.GetValue("src").ToString();
                    try
                    {
                        _imageCropperInfoService.ValidateFileExtension(path);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    TinifyImageCroppers(path, nonOptimizedImages, imageCropperInfo, key);
                }
            }
        }

        public MenuItemCollection GetMenuForNode(string timageId)
        {
            // var menu = new MenuItemCollection();
            //
            // var menuItem = new Umbraco.Web.Models.Trees.MenuItem();
            // menuItem.Alias = "Menu Item 1";
            // menuItem.Icon = "heard";
            //
            //     // root actions, perhaps users can create new items in this tree, or perhaps it's not a content tree, it might be a read only tree, or each node item might represent something entirely different...
            //     // add your menu item actions or custom ActionMenuItems
            // menu.Items.Add(new CreateChildEntity("Test"));
            //     menu.Items.Add(new ExportMember(Services.TextService));
            //     menu.Items.Add(menuItem);
            //     // add refresh menu item (note no dialog)            
            //     menu.Items.Add(new RefreshNode(Services.TextService, true));
            //     return menu;

            return null;
        }
    }
}