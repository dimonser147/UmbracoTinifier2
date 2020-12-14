using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Infrastructure.Exceptions;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.FileSystemProvider;
using Tinifier.Core.Services;
using Tinifier.Core.Services.History;
using Tinifier.Core.Services.ImageCropperInfo;
using Tinifier.Core.Services.Media;
using Tinifier.Core.Services.Settings;
using Tinifier.Core.Services.Statistic;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.JavaScript;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Tinifier.Core.Application
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    class tinifierStartup : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<SectionService>();
        }
    }

    public class SectionService : IComponent
    {
        private readonly IFileSystemProviderRepository _fileSystemProviderRepository;
        private readonly ISettingsService _settingsService;
        private readonly IStatisticService _statisticService;
        private readonly IImageService _imageService;
        private readonly IHistoryService _historyService;
        private readonly IImageCropperInfoService _imageCropperInfoService;

        public SectionService(IFileSystemProviderRepository fileSystemProviderRepository, ISettingsService settingsService,
            IStatisticService statisticService, IImageService imageService, IHistoryService historyService)
        {
            _fileSystemProviderRepository = fileSystemProviderRepository;
            _settingsService = settingsService;
            _statisticService = statisticService;
            _imageService = imageService;
            _historyService = historyService;
            // _imageCropperInfoService = imageCropperInfoService;
        }

        public void Initialize()
        {
            SetFileSystemProvider();
            ServerVariablesParser.Parsing += Parsing;
            TreeControllerBase.MenuRendering += MenuRenderingHandler;

            MediaService.Saved += MediaService_Saved;
            MediaService.Saving += MediaService_Saving;
            ContentService.Saving += ContentService_Saving;

            MediaService.Deleted += MediaService_Deleted;
            MediaService.EmptiedRecycleBin += MediaService_EmptiedRecycleBin;

            ServerVariablesParser.Parsing += Parsing;
        }

        private void MediaService_Deleted(IMediaService sender, DeleteEventArgs<IMedia> e)
        {
            foreach (var item in e.DeletedEntities)
                _historyService.Delete(item.Id.ToString());

            _statisticService.UpdateStatistic(e.DeletedEntities.Count());
        }

        public void Terminate()
        {
        }

        /// <summary>
        /// Update number of images in statistic before removing from recyclebin
        /// </summary>
        /// <param name="sender">IMediaService</param>
        /// <param name="e">RecycleBinEventArgs</param>
        private void MediaService_EmptiedRecycleBin(IMediaService sender, RecycleBinEventArgs e)
        {

            //foreach (var id in e.)
            //{
            //    _historyService.Delete(id.ToString());
            //}
            //if (e.Ids.Any())
            //    _statisticService.UpdateStatistic(e.Ids.Count());
        }

        private void ContentService_Saving(IContentService sender, SaveEventArgs<IContent> e)
        {
            var settingService = _settingsService.GetSettings();
            if (settingService == null)
                return;

            foreach (var entity in e.SavedEntities)
            {
                var imageCroppers = entity.Properties.Where(x => x.PropertyType.PropertyEditorAlias ==
                                                                 Constants.PropertyEditors.Aliases.ImageCropper);

                foreach (Property crop in imageCroppers)
                {
                    var key = string.Concat(entity.Name, "-", crop.Alias);
                    var imageCropperInfo = _imageCropperInfoService.Get(key);
                    var imagePath = crop.GetValue();

                    //Wrong object
                    if (imageCropperInfo == null && imagePath == null)
                        continue;

                    //Cropped file was Deleted
                    if (imageCropperInfo != null && imagePath == null)
                    {
                        _imageCropperInfoService.DeleteImageFromImageCropper(key, imageCropperInfo);
                        continue;
                    }

                    var json = JObject.Parse(imagePath.ToString());
                    var path = json.GetValue("src").ToString();

                    //republish existed content
                    if (imageCropperInfo != null && imageCropperInfo.ImageId == path)
                        continue;

                    //Cropped file was created or updated
                    _imageCropperInfoService.GetCropImagesAndTinify(key, imageCropperInfo, imagePath,
                        settingService.EnableOptimizationOnUpload, path);
                }
            }
        }

        private void MediaService_Saving(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            MediaSavingHelper.IsSavingInProgress = true;
            // reupload image issue https://goo.gl/ad8pTs
            HandleMedia(e.SavedEntities,
                    (m) => {
                        _historyService.Delete(m.Id.ToString());
                        return false;
                    },
                    (m) => m.IsPropertyDirty(PackageConstants.UmbracoFileAlias));
        }

        private void MediaService_Saved(IMediaService sender, SaveEventArgs<IMedia> e)
        {
            // MediaSavingHelper.IsSavingInProgress = false;
            // // optimize on upload
            var settingService = _settingsService.GetSettings();
            if (settingService == null || settingService.EnableOptimizationOnUpload == false)
                return;

            HandleMedia(e.SavedEntities,
                (m) =>
                {
                    try
                    {
                        OptimizeOnUploadAsync(m.Id, e).GetAwaiter().GetResult();
                        return true;
                    }
                    catch (NotSupportedExtensionException)
                    { }

                    return false;
                });
        }

        private void HandleMedia(IEnumerable<IMedia> items, Func<IMedia, bool> action, Func<IMedia, bool> predicate = null)
        {
            var isChanged = false;
            foreach (var item in items)
            {
                if (string.Equals(item.ContentType.Alias, PackageConstants.ImageAlias, StringComparison.OrdinalIgnoreCase))
                {
                    if (action != null && (predicate == null || predicate(item)))
                    {
                        isChanged |= action(item);                        
                    }
                }
            }

            //Try to limit the amount of times the statistics are gathered
            if (isChanged)
            {
                Task.Run(() => _statisticService.UpdateStatistic());
            }
        }

        /// <summary>
        /// Call methods for tinifing when upload image
        /// </summary>
        /// <param name="mediaItemId">Media Item Id</param>
        /// <param name="e">CancellableEventArgs</param>
        private async System.Threading.Tasks.Task OptimizeOnUploadAsync(int mediaItemId, CancellableEventArgs e)
        {
            TImage image;

            try
            {
                image = _imageService.GetImage(mediaItemId);
            }
            catch (NotSupportedExtensionException ex)
            {
                e.Messages.Add(new EventMessage(PackageConstants.ErrorCategory, ex.Message,
                    EventMessageType.Error));
                throw;
            }

            var imageHistory = _historyService.GetImageHistory(image.Id);

            if (imageHistory == null)
                await _imageService.OptimizeImageAsync(image).ConfigureAwait(false);
        }

        private void MenuRenderingHandler(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            if (string.Equals(sender.TreeAlias, PackageConstants.MediaAlias, StringComparison.OrdinalIgnoreCase))
            {
                var menuItemTinifyButton = new MenuItem(PackageConstants.TinifierButton, PackageConstants.TinifierButtonCaption);
                menuItemTinifyButton.LaunchDialogView(PackageConstants.TinyTImageRoute, PackageConstants.SectionName);
                menuItemTinifyButton.Icon = PackageConstants.MenuIcon;
                e.Menu.Items.Add(menuItemTinifyButton);

                var menuItemUndoTinifyButton = new MenuItem(PackageConstants.UndoTinifierButton, PackageConstants.UndoTinifierButtonCaption);
                menuItemUndoTinifyButton.LaunchDialogView(PackageConstants.UndoTinyTImageRoute, PackageConstants.UndoTinifierButtonCaption);
                menuItemUndoTinifyButton.Icon = PackageConstants.UndoTinifyIcon;
                e.Menu.Items.Add(menuItemUndoTinifyButton);

                var menuItemSettingsButton = new MenuItem(PackageConstants.StatsButton, PackageConstants.StatsButtonCaption);
                menuItemSettingsButton.LaunchDialogView(PackageConstants.TinySettingsRoute, PackageConstants.StatsDialogCaption);
                menuItemSettingsButton.Icon = PackageConstants.MenuSettingsIcon;
                e.Menu.Items.Add(menuItemSettingsButton);

                var menuItemOrganizeImagesButton = new MenuItem(PackageConstants.OrganizeImagesButton, PackageConstants.OrganizeImagesCaption);
                menuItemOrganizeImagesButton.LaunchDialogView(PackageConstants.OrganizeImagesRoute, PackageConstants.OrganizeImagesCaption);
                e.Menu.Items.Add(menuItemOrganizeImagesButton);
            }
        }

        private void SetFileSystemProvider()
        {

            var path = HostingEnvironment.MapPath("~/Web.config");
            var doc = new XmlDocument();
            doc.Load(path);

            XmlNode xmlNode = doc.DocumentElement.SelectSingleNode("appSettings");
            XmlNodeList xmlList = xmlNode.SelectNodes("add");

            var fileSystemType = "PhysicalFileSystem";
            foreach (XmlNode xmlNodeS in xmlList)
            {
                if (xmlNodeS.Attributes.GetNamedItem("key").Value.Contains("AzureBlobFileSystem"))
                {
                    fileSystemType = "AzureBlobFileSystem";
                    break;
                }
            }

            _fileSystemProviderRepository.Delete();
            _fileSystemProviderRepository.Create(fileSystemType);
        }

        private void Parsing(object sender, Dictionary<string, object> dictionary)
        {
            //var umbracoPath = WebConfigurationManager.AppSettings["umbracoPath"];
            //
            //var apiRoot = $"{umbracoPath.Substring(1)}/backoffice/api/";
            //
            //var urls = dictionary["umbracoUrls"] as Dictionary<string, object>;
            //urls["tinifierApiRoot"] = apiRoot;
        }
    }
}

