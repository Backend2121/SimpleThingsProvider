﻿using DownloaderExtension;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleThingsProvider
{
    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress, CancellationToken cancellationToken = default, IsPaused isPaused = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            try
            {
                while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    if (isPaused.pause)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                    totalBytesRead += bytesRead;
                    progress.Report(totalBytesRead * 100);
                }
            }
            catch (TaskCanceledException e)
            {
                if (destination is FileStream fileStream)
                {
                    try
                    {
                        // Delete the file
                        destination.Close();
                        Debug.WriteLine(fileStream.Name);
                        File.Delete(fileStream.Name);
                        return;
                    }
                    catch(Exception c) { Debug.WriteLine(c); }
                }
                return;
            }
            
        }
    }
}