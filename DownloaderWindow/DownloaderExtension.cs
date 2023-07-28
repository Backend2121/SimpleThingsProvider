using DownloaderExtension;
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
        public string Name { get { return "Downloader"; } set { } }
        public Window extensionWindow { get; set; }
        private global::DownloaderExtension.DownloaderWindow dw;
        public DownloaderExtension()
        {
            extensionWindow = new global::DownloaderExtension.DownloaderWindow();
            dw = (global::DownloaderExtension.DownloaderWindow)extensionWindow;
        }
        public Window getExtensionWindow() 
        {
            // Reload previous state
            if (dw == null)
            {
                dw = new global::DownloaderExtension.DownloaderWindow();
            }
            return dw;
        }
        /// <summary>
        /// args[0] string
        /// args[1] string
        /// </summary>
        public void startFunction(object[] args)
        {
            // Check if args is empty
            if (args.Length > 1)
            {
                if (args[0] != null && args[1] != null)
                {
                    dw.addDownload(args[0].ToString(), args[1].ToString());
                }
            }
        }
    }
}