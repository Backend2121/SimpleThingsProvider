using DownloaderWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleThingsProvider
{
    internal class DownloaderExtension : IExtension
    {
        private string fileName;
        private string uri;
        public string Name { get { return "Downloader"; } set { } }
        public Window extentionWindow { get; set; }
        public DownloaderExtension()
        {
            extentionWindow = new DownloaderWindow.DownloaderWindow();
        }
        public void startDownload(string name, string url)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.DownloadFileAsync(
                    // Param1 = Link of file
                    new System.Uri("https://dl2.hexrom.com/rom/3ds/Mario%20Party%20-%20The%20Top%20100%20(Europe)%20(En_Fr_De_Es_It_Nl)6130_hexrom.com_.zip"),
                    // Param2 = Path to save
                    "C:\\Users\\alexi\\Desktop\\mario.zip"
                );
            }
            ((DownloaderWindow.DownloaderWindow)extentionWindow).FileName.Content = name;
        }
        public void setParameters(object[] args)
        {
            fileName = (string)args[0];
            uri = args[1].ToString();
        }
        public void startFunction()
        {
            startDownload(fileName, uri);
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ((DownloaderWindow.DownloaderWindow)extentionWindow).Progress.Value = e.ProgressPercentage;
            ((DownloaderWindow.DownloaderWindow)extentionWindow).Percentage.Content = e.ProgressPercentage + "%";
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ((DownloaderWindow.DownloaderWindow)extentionWindow).Progress.Value = 100;
            ((DownloaderWindow.DownloaderWindow)extentionWindow).Percentage.Content = "100%";
        }
    }
}