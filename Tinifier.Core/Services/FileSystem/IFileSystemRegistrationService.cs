using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.IO;

namespace Tinifier.Core.Services.FileSystem
{
    public interface IFileSystemRegistrationService
    {
        IFileSystem GetFileSystem();
    }
}
