using System;
using System.Collections.Generic;
using Tinifier.Core.Infrastructure;
using Tinifier.Core.Infrastructure.Enums;
using Tinifier.Core.Infrastructure.Exceptions;
using Tinifier.Core.Models.Db;
using Tinifier.Core.Repository.Image;
using Tinifier.Core.Repository.State;

namespace Tinifier.Core.Services.Validation
{
    public class ValidationService : IValidationService
    {
        private readonly IStateRepository _stateRepository;
        private readonly IImageRepository _imageRepository;

        public ValidationService(IStateRepository stateRepository, IImageRepository imageRepository)
        {
            _stateRepository = stateRepository;
            _imageRepository = imageRepository;
        }

        public void ValidateConcurrentOptimizing()
        {
            var state = _stateRepository.Get((int)Statuses.InProgress);

            if (state != null)
                throw new ConcurrentOptimizingException(PackageConstants.ConcurrentOptimizing);
        }

        public bool IsFolder(int itemId)
        {
            var item = _imageRepository.Get(itemId);

            return string.Equals(item.ContentType.Alias, PackageConstants.FolderAlias, StringComparison.OrdinalIgnoreCase);
        }

        public void ValidateExtension(Umbraco.Core.Models.Media media)
        {
            if (media == null)
                throw new EntityNotFoundException();
            if (!media.HasProperty("umbracoExtension"))
                throw new NotSupportedExtensionException();
            var fileExt = media.GetValue<string>("umbracoExtension");
            foreach (var supportedExt in PackageConstants.SupportedExtensions)
                if (string.Equals(supportedExt, fileExt, StringComparison.OrdinalIgnoreCase))
                    return;
            throw new NotSupportedExtensionException(fileExt);
        }
    }
}
