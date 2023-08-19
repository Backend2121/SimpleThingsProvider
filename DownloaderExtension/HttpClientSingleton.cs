using DownloaderExtension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleThingsProvider
{
    public static class HttpClientSingleton
    {
        public static HttpClient client = new HttpClient();
        static HttpClientSingleton()
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        }
        public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default, IsPaused isPaused = null)
        {
            // Get the http headers first to examine the content length
            using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
            {
                var contentLength = response.Content.Headers.ContentLength;

                using (var download = await response.Content.ReadAsStreamAsync())
                {
                    // Ignore progress reporting when no progress reporter was 
                    // passed or when the content length is unknown
                    if (progress == null || !contentLength.HasValue)
                    {
                        await download.CopyToAsync(destination);
                        return;
                    }

                    // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
                    var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
                    // Use extension method to report progress while downloading
                    await download.CopyToAsync(destination, 8192, relativeProgress, cancellationToken, isPaused);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        progress.Report(0);
                    }
                    else
                    {
                        progress.Report(100);
                    }
                    destination.Close();
                }
            }
        }
    }
}