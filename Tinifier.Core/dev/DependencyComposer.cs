using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tinifier.Core.Controllers;
using Umbraco.Core.Composing;
using Umbraco.Core;
using Tinifier.Core.Services.Validation;
using Tinifier.Core.Repository.State;
using Umbraco.Core.Persistence;
using Tinifier.Core.Repository.Image;
using Tinifier.Core.Repository.History;
using Tinifier.Core.Services.Media;
using Tinifier.Core.Services.Settings;
using Tinifier.Core.Repository.Settings;
using Tinifier.Core.Services.History;
using Tinifier.Core.Services.State;
using Tinifier.Core.Services.TinyPNG;
using Tinifier.Core.Repository.FileSystemProvider;
using Tinifier.Core.Services.Statistic;
using Tinifier.Core.Repository.Statistic;
using Tinifier.Core.Services.Media.Organizers;
using Tinifier.Core.Services.ImageCropperInfo;
using Tinifier.Core.Repository.Common;
using Tinifier.Core.Services.FileSystem;

namespace Tinifier.Core.dev
{
    public class DependencyComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IDeveloperLogger, DeveloperLogger>();
            composition.Register<IUmbracoDatabase, UmbracoDatabase>();
            composition.Register<ITinyPNGConnector, TinyPNGConnectorService>();

            composition.Register<IValidationService, ValidationService>();
            composition.Register<ITinyImageService, TinyImageService>();
            composition.Register<IStatisticService, StatisticService>();
            composition.Register<ISettingsService, SettingsService>();
            composition.Register<IStateService, StateService>();
            composition.Register<IImageHistoryService, TImageHistoryService>();
            composition.Register<IHistoryService, HistoryService>();
            composition.Register<IImageService, TImageService>();
            composition.Register<IImageCropperInfoService, ImageCropperInfoService>();


            composition.Register<IStateRepository, TStateRepository>();
            composition.Register<IImageRepository, TImageRepository>();
            composition.Register<IHistoryRepository, THistoryRepository>();
            composition.Register<ISettingsRepository, TSettingsRepository>();
            composition.Register<IFileSystemProviderRepository, TFileSystemProviderRepository>();
            composition.Register<IImageHistoryRepository, TImageHistoryRepository>();
            composition.Register<IStatisticRepository, TStatisticRepository>();
            composition.Register<IMediaHistoryRepository, TMediaHistoryRepository>();
            composition.Register<IUmbracoDbRepository, UmbracoDbRepository>();

            composition.Register<IImageOrganizer, ByUploadedDateImageOrganizer>();
            composition.Register<IFileSystemRegistrationService, FileSystemRegistrationService>();
        }
    }
}
