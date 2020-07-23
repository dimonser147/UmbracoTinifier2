using System.Net;

namespace Tinifier.Core.Services.TinyPNG
{
    public sealed class TinyImageService : ITinyImageService
    {
        public byte[] DownloadImage(string url)
        {
            byte[] tinyImageBytes;

            using (var webClient = new WebClient())
            {
                tinyImageBytes = webClient.DownloadData(url);
            }
            return tinyImageBytes;
        }
    }
}
