using System;
using System.Configuration;
using Our.Umbraco.FileSystemProviders.Azure;
using Tinifier.Core.Repository.FileSystemProvider;
using Umbraco.Core.IO;

namespace Tinifier.Core.Services.FileSystem
{
    public class FileSystemRegistrationService : IFileSystemRegistrationService
    {
        private readonly IFileSystemProviderRepository _fileSystemProviderRepository;

        public FileSystemRegistrationService(IFileSystemProviderRepository fileSystemProviderRepository)
        {
            _fileSystemProviderRepository = fileSystemProviderRepository;
        }

        public IFileSystem GetFileSystem()
        {
            IFileSystem fileSystem = null;

            var fs = _fileSystemProviderRepository.GetFileSystem();
            if (fs != null)
            {
                if (fs.Type == "PhysicalFileSystem")
                {
                    fileSystem = new PhysicalFileSystem("~/media");
                }
                else
                {
                    try
                    {
                        fileSystem = AzureFileSystem.GetInstance(
                            ConfigurationManager.AppSettings["AzureBlobFileSystem.ContainerName:media"],
                            ConfigurationManager.AppSettings["AzureBlobFileSystem.RootUrl:media"],
                            ConfigurationManager.AppSettings["AzureBlobFileSystem.ConnectionString:media"],
                            ConfigurationManager.AppSettings["AzureBlobFileSystem.MaxDays:media"],
                            ConfigurationManager.AppSettings["AzureBlobFileSystem.UseDefaultRoute:media"],
                            ConfigurationManager.AppSettings["AzureBlobFileSystem.UsePrivateContainer:media"]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            return fileSystem ?? new PhysicalFileSystem("~/media");
        }
    }
}
