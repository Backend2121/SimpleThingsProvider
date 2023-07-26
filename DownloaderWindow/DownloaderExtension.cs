using DownloaderWindow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleThingsProvider
{
    internal class DownloaderExtension : IExtension
    {
        private string fileName;
        private string uri;
        private Regex extensionExpression = new("(\\.)+(.{2,4})(?!.*\\1)");
        public string Name { get { return "Downloader"; } set { } }
        public Window extensionWindow { get; set; }
        public DownloaderExtension()
        {
            extensionWindow = new DownloaderWindow.DownloaderWindow();
        }
        public Window getExtensionWindow() 
        {
            // Reload previous state
            DownloaderWindow.DownloaderWindow w = new DownloaderWindow.DownloaderWindow();
            return w;
        }
        public void startDownload(string name, string url)
        {
            Match match = extensionExpression.Match(url);
            if (match.Success)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += client_DownloadProgressChanged;
                    client.DownloadFileCompleted += client_DownloadFileCompleted;
                    client.DownloadFileAsync(
                        // Param1 = Link of file
                        new System.Uri(url),
                        // Param2 = Path to save
                        "C:\\Users\\alexi\\Desktop\\" + name + match.Value
                    );
                }
                //((DownloaderWindow.DownloaderWindow)extensionWindow).FileName.Content = name;
            }
            else
            {
                //Regex match not found, notify user
            }
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
            //((DownloaderWindow.DownloaderWindow)extensionWindow).Progress.Value = e.ProgressPercentage;
            //((DownloaderWindow.DownloaderWindow)extensionWindow).Percentage.Content = e.ProgressPercentage + "%";
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //((DownloaderWindow.DownloaderWindow)extensionWindow).Progress.Value = 100;
            //((DownloaderWindow.DownloaderWindow)extensionWindow).Percentage.Content = "100%";
        }
    }
}