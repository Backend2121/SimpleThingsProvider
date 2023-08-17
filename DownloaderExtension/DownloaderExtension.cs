using DownloaderExtension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SimpleThingsProvider
{
    internal class DownloaderExtension : IExtension
    {
        public string Name { get { return "Downloader"; } set { } }
        public string ExtensionVersion { get { return "1.0.0"; } set { } }
        private Regex _extensionExpression = new("(\\.)(jpg|JPG|gif|GIF|doc|DOC|pdf|PDF|zip|ZIP|rar|RAR|7z|7Z)$");
        private string _configFileName = "Downloader_Config";
        private System.Windows.Controls.TextBox downloadPathTB;
        public Window extensionWindow { get; set; }
        private DownloaderWindow dw;
        private System.Windows.Controls.Button downloadButton;
        private System.Windows.Controls.ListView lv;
        private System.Windows.Controls.ListView lv2;
        private System.Windows.Controls.Label ol;
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

            result = System.Windows.MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
        }

        public void enableButton(System.Windows.Controls.Label outputLabel)
        {
            if (_extensionExpression.Match(outputLabel.Content.ToString()).Success)
            {
                downloadButton.IsEnabled = true;
            }
            else
            {
                Debug.WriteLine(outputLabel.Content.ToString());
                downloadButton.IsEnabled = false;
            }
        }

        public void disableButton(System.Windows.Controls.Label outputLabel)
        {
            downloadButton.IsEnabled = false;
        }

        public List<UIElement> getElements(System.Windows.Controls.ListView listView, System.Windows.Controls.Label label)
        {
            lv = listView;
            ol = label;
            List<UIElement> list = new List<UIElement>();
            // Button
            downloadButton = new System.Windows.Controls.Button();
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

        public List<DockPanel> getSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            JsonSettings jsonSettings = new JsonSettings("Configs", _configFileName, settings);
            settings = jsonSettings.loadFromJson();
            List<DockPanel> dockPanels = new List<DockPanel>();
            DockPanel row1 = new DockPanel();
            DockPanel row2 = new DockPanel();
            Grid grid1 = new Grid();
            ColumnDefinition column1Definition1 = new ColumnDefinition();
            ColumnDefinition column1Definition2 = new ColumnDefinition();
            ColumnDefinition column1Definition3 = new ColumnDefinition();
            column1Definition1.Width = new GridLength(1, GridUnitType.Star);
            column1Definition2.Width = new GridLength(3, GridUnitType.Star);
            column1Definition3.Width = new GridLength(1, GridUnitType.Star);
            grid1.ColumnDefinitions.Add(column1Definition1);
            grid1.ColumnDefinitions.Add(column1Definition2);
            grid1.ColumnDefinitions.Add(column1Definition3);

            // Labels
            System.Windows.Controls.Label downloadPathLabel = new System.Windows.Controls.Label();
            System.Windows.Controls.Label tempDownloadPathLabel = new System.Windows.Controls.Label();
            downloadPathLabel.Content = "Download path: ";
            downloadPathLabel.Margin = new Thickness(10, 10, 10, 10);
            tempDownloadPathLabel.Content = "Temp files location: ";
            tempDownloadPathLabel.Margin = new Thickness(10, 10, 10, 10);

            // TextBoxes
            downloadPathTB = new System.Windows.Controls.TextBox();
            downloadPathTB.IsReadOnly = true;
            downloadPathTB.Margin = new Thickness(10, 10, 10, 10);
            try
            {
                downloadPathTB.Text = settings["downloadPath"];
            }
            catch { }

            // Path Pickers
            System.Windows.Controls.Button downloadPathPicker = new System.Windows.Controls.Button();
            downloadPathPicker.Name = "downloadPathPicker";
            downloadPathPicker.Content = "...";
            downloadPathPicker.Click += PathPicker_Click;
            downloadPathPicker.Margin = new Thickness(10, 10, 10, 10);
            // Set to columns
            Grid.SetColumn(downloadPathLabel, 0);
            Grid.SetColumn(tempDownloadPathLabel, 0);
            Grid.SetColumn(downloadPathTB, 1);
            Grid.SetColumn(downloadPathPicker, 2);
            // Add childrens to grid
            grid1.Children.Add(downloadPathLabel);
            grid1.Children.Add(downloadPathTB);
            grid1.Children.Add(downloadPathPicker);
            row1.Children.Add(grid1);
            dockPanels.Add(row1);
            dockPanels.Add(row2);
            return dockPanels;
        }

        public void PathPicker_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBroserDialog = new FolderBrowserDialog();
            folderBroserDialog.Description = "Choose a path";

            // Mostra il selettore di percorsi e verifica se l'utente ha selezionato un percorso valido
            DialogResult result = folderBroserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (((System.Windows.Controls.Button)sender).Name.Equals("downloadPathPicker"))
                {
                    downloadPathTB.Text = folderBroserDialog.SelectedPath;
                }
            }
        }

        public void saveSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings.Add("downloadPath", downloadPathTB.Text);
            JsonSettings jsonSettings = new JsonSettings("Configs", _configFileName, settings);
            jsonSettings.saveToJson();
        }

        public async void checkUpdate()
        {
            string repoURL = "https://raw.githubusercontent.com/Backend2121/SimpleThingsProvider/Development/DownloaderExtension/Info.json";
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, repoURL);
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("User-Agent", "SimpleThingsProvider"));
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string content = await response.Content.ReadAsStringAsync();
            int start = content.IndexOf(": \"");
            int end = content.IndexOf('"', start + 3);
            string version = content.Substring(start + 3, end - start - 3);
            if (!version.Equals(ExtensionVersion))
            {
                MessageBoxResult r = AlertClass.Alert("An update for " + Name + " is available, open the GitHub page?", Name, MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (r == MessageBoxResult.Yes)
                {
                    var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://github.com/Backend2121/SimpleThingsProvider/releases/latest" };
                    System.Diagnostics.Process.Start(process);
                    Logger.Log($"Newer version for {Name}: {version} found!", Name + " Updater");
                }
            }
        }
    }
}