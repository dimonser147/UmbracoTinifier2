using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Models;
using Tinifier.Core.Models.API;
using Tinifier.Core.Repository.Common;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using uMedia = Umbraco.Core.Models.Media;

namespace Tinifier.Core.Services.Media.Organizers
{
    public interface IImageOrganizer
    {
        void Organize(int sourceFolderId);
        void CheckConstraints(int sourceFolderId);
    }


    public class ByUploadedDateImageOrganizer : IImageOrganizer
    {
        protected readonly IImageService _imageService;
        private readonly IMediaService _mediaService;
        private readonly IUmbracoDbRepository _umbracoDbRepository;

        protected List<OrganisableMediaModel> _previewModel;


        public ByUploadedDateImageOrganizer(IImageService imageService, IMediaService mediaService, IUmbracoDbRepository umbracoDbRepository)
        {
            _imageService = imageService;
            _mediaService = mediaService;
            _previewModel = new List<OrganisableMediaModel>();
            _umbracoDbRepository = umbracoDbRepository;
        }

        public void Organize(int sourceFolderId)
        {
            CheckConstraints(sourceFolderId);
            PreparePreviewModel(sourceFolderId);
            SaveCurrentState(sourceFolderId);
            ProcessPreviewModel(sourceFolderId);
        }

        private void SaveCurrentState(int sourceFolderId)
        {
            var media = _imageService.GetAllImagesAt(sourceFolderId);
            _imageService.BackupMediaPaths(media);
        }

        private void ProcessPreviewModel(int sourceFolderId)
        {
            foreach (var item in _previewModel)
            {
                int parentFolderId = sourceFolderId;
                string year = item.DestinationPath[0];
                string month = item.DestinationPath[1];
                IMedia folderYear;
                IMedia folderMonth;


                List<UmbracoNode> nodesYear = _umbracoDbRepository.GetNodesByName(year);
                //var checkIfFolderYearExists = Query.GetMediaByName(year).FirstOrDefault(s => s.ParentId == parentFolderId);
                var checkIfFolderYearExists = nodesYear.FirstOrDefault(s => s.ParentId == parentFolderId);
                if (nodesYear == null || nodesYear.Count() == 0)
                {
                    folderYear = _mediaService.CreateMediaWithIdentity(year, parentFolderId, PackageConstants.FolderAlias);
                    parentFolderId = folderYear.Id;
                }
                else
                {
                    var media = _mediaService.GetById(nodesYear.First().Id);
                    parentFolderId = media.Id;
                }

                List<UmbracoNode> nodesMounth = _umbracoDbRepository.GetNodesByName(month);

               // var checkIfFolderMonthExists = nodesMounth.FirstOrDefault(s => s.ParentId == parentFolderId);
                if (nodesMounth == null || nodesMounth.Count() == 0)
                {
                    folderMonth = _mediaService.CreateMediaWithIdentity(month, parentFolderId, PackageConstants.FolderAlias);
                    parentFolderId = folderMonth.Id;
                }
                else
                {
                    var media = _mediaService.GetById(nodesMounth.First().Id);
                    parentFolderId = media.Id;
                }
                _imageService.Move(item.Media, parentFolderId);
            }
        }

        private void PreparePreviewModel(int sourceFolderId)
        {
            var media = _imageService.GetAllImagesAt(sourceFolderId);

            foreach (var m in media)
            {
                var creationDate = m.CreateDate.ToLocalTime();
                string year = creationDate.Year.ToString();
                string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(creationDate.Month);

                _previewModel.Add(new OrganisableMediaModel
                {
                    Media = m,
                    DestinationPath = new string[] { year, month }
                });
            };
        }

        public void CheckConstraints(int sourceFolderId)
        {
            if (_imageService.IsFolderChildOfOrganizedFolder(sourceFolderId))
                throw new OrganizationConstraintsException(@"This folder is already organized.");
        }
    }
}
