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
        public string name { get { return "Downloader"; } set { } }
        public Window extensionWindow { get; set; }
        private DownloaderWindow dw;
        private Button downloadButton;
        private ListView lv;
        private Label ol;
        public DownloaderExtension()
        {
            extensionWindow = new DownloaderWindow();
            dw = (DownloaderWindow)extensionWindow;
        }
        public Window getExtensionWindow() 
        {
            // Reload previous state
            if (dw == null)
            {
                dw = new DownloaderWindow();
            }
            return dw;
        }
        /// <summary>
        /// args[0] string
        /// args[1] string
        /// </summary>
        public bool startFunction(object[] args)
        {
            // Check if args is empty
            if (args.Length > 1)
            {
                if (args[0] != null && args[1] != null && args[1].ToString().Contains("http"))
                {
                    Debug.WriteLine("Title: " + args[0]);
                    Debug.WriteLine("URL: " + args[0]);
                    dw.addDownload(args[0].ToString(), args[1].ToString());
                    return true;
                }
                else
                {
                    Alert("Selected entry is invalid", "Error");
                    return false;
                }
            }
            return false;
        }
        public void Alert(string messageBoxText, string caption)
        {
            Logger.Log(messageBoxText, "Alert");
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
        }

        public void enableButton()
        {
            downloadButton.IsEnabled = true;
        }

        public void disableButton()
        {
            downloadButton.IsEnabled = false;
        }

        public List<UIElement> getElements(ListView listView, Label label)
        {
            lv = listView;
            ol = label;
            List<UIElement> list = new List<UIElement>();
            // Button
            downloadButton = new Button();
            downloadButton.Content = "Download";
            downloadButton.IsEnabled = false;
            downloadButton.Margin = new Thickness(10, 2, 10, 2);
            downloadButton.Click += DownloadButton_Click;

            // Menu entry

            list.Add(downloadButton);
            return list;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Pass the new download
            object[] args = { ((Result)lv.SelectedItem).Title, ol.Content.ToString() };
            if (startFunction(args))
            {
                // Show the downloader window
                dw.Show();
                dw.Focus();
            }
        }

        public void showWindow()
        {
            // Show the downloader window
            dw.Show();
            dw.Focus();
        }
    }
}