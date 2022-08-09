using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using System.Threading;
using System.Xml;

namespace SimpleThingsProvider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        List<string> underlying;
        Websites module = new Websites();
        public MainWindow()
        {
            Application.Current.MainWindow = this;
            InitializeComponent();

            Logger.Log("Initialized MainWindow", "Main");

            WebsiteSource.SelectedIndex = Settings.Default.WebsiteSelected;
            WebsiteSubSelector.SelectedIndex = Settings.Default.WebsiteSubSelected;

            Logger.Log($"Loaded settings: {WebsiteSource.SelectedIndex}___{WebsiteSubSelector.SelectedIndex}", "Main");

            if (WebsiteSource.SelectedItem.ToString() == "WoWRoms") { WebsiteSubSelector.IsEnabled = true; }
            else { WebsiteSubSelector.IsEnabled = false; }
            try
            {
                Logger.Log("Searching for new updates", "Updater");
                checkUpdate();
            }
            catch (Exception e)
            {
                if (e.ToString() == "ArgumentNullException")
                {
                    Logger.Log($"{e}", "Exception Handler");
                }
                if (e.ToString() == "WebException")
                {
                    Logger.Log($"{e}", "Exception Handler");
                }
            }
            Logger.Log("Everything ready!", "Main");
        }
        
        private void checkUpdate()
        {
            string repoURL = "https://raw.githubusercontent.com/Backend2121/SimpleThingsProvider/master/TorrentScraper/Settings.settings";

            WebClient webClient = new WebClient();
            Stream stream = webClient.OpenRead(repoURL);
            StreamReader reader = new StreamReader(stream);
            String content = reader.ReadToEnd();

            var start = content.IndexOf("ApplicationVersion");
            var end = content.IndexOf("</Value>", start);
            var start2 = content.IndexOf("(Default)", start);
            var version = content.Substring(start2 + 11, end - start2 - 11);
            if (version != Settings.Default.ApplicationVersion)
            {
                if (UpdateAvailable() == MessageBoxResult.Yes)
                {
                    var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://github.com/Backend2121/SimpleThingsProvider/releases/latest" };
                    System.Diagnostics.Process.Start(process);
                    Logger.Log($"Newer version: {version} found!", "Updater");
                    Close();
                }
            }
        }
        public class Result
        {
            public string Title { get; set; }
            public string Seeds { get; set; }
            public string Leechs { get; set; }
            public string Time { get; set; }
            public string Size { get; set; }
        }
        private void Search(object sender, RoutedEventArgs e)
        {
            // Reset to default
            OpenInBrowserButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            OutputLabel.Content = "Output";
            string whoami = WebsiteSource.SelectedItem.ToString();

            HttpStatusCode code = module.Search(SearchTextBox.Text, whoami);
            if (code != HttpStatusCode.OK) { Alert("Received a non 200(OK) response!" + "\n" + code, "STD: Error"); StatusCodeLabel.Content = "Status Code: " + code; StatusCodeLabel.Foreground = new SolidColorBrush(Colors.Red); return; }
            else { underlying = module.getResults(module.doc, ResultsList, SearchTextBox.Text); StatusCodeLabel.Content = "Status Code: " + code; StatusCodeLabel.Foreground = new SolidColorBrush(Colors.Green); }
            if (underlying.Count <= 0)
            {
                Alert("No results found!", "STD: Warning");
                ResultsNumber.Content = "Results: 0";
                return;
            }
            else{ ResultsNumber.Content = "Results: " + underlying.Count;}
        }
        public void Alert(string messageBoxText, string caption)
        {
            Logger.Log(messageBoxText, "Alert");
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
        }
        public MessageBoxResult UpdateAvailable()
        {
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Question;
            MessageBoxResult result;

            return MessageBox.Show("A new version of this software has been found, do you want to open the download page?", "UPDATE", button, icon, MessageBoxResult.Yes);
        }
        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HtmlWeb web = new HtmlWeb();
            try
            {
                var entry = module.getMagnet(ResultsList.SelectedIndex);
                Logger.Log($"Entry {entry} has been selected", "Main");
                OutputLabel.Content = entry;
                CopyButton.IsEnabled = true;
                OpenInBrowserButton.IsEnabled = true;
                return;
            }
            catch (ArgumentOutOfRangeException) { CopyButton.IsEnabled = true; OpenInBrowserButton.IsEnabled = false; OutputLabel.Content = "Output"; return; }
        }
        private void Copy(object sender, RoutedEventArgs e)
        {
            Logger.Log($"User copied {OutputLabel.Content.ToString()}", "Main");
            Clipboard.SetText(OutputLabel.Content.ToString());
        }
        private void OpenWebsiteStatus(object sender, RoutedEventArgs e)
        {
            WebsiteStatusWindow websiteStatusWindow = new WebsiteStatusWindow();
            websiteStatusWindow.Show();
            websiteStatusWindow.Focus();
        }
        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
            aboutWindow.Focus();
        }
        private void OpenProxyWindow(object sender, RoutedEventArgs e)
        {
            ProxyWindow proxyWindow = new ProxyWindow();
            proxyWindow.Show();
            proxyWindow.Focus();
        }
        private void SaveSelected(object sender, RoutedEventArgs e)
        {
            Logger.Log("Saving settings", "Main");
            if (WebsiteSource.SelectedItem.ToString() == "WoWRoms") { WebsiteSubSelector.IsEnabled = true; }
            else { WebsiteSubSelector.IsEnabled = false; }
            Settings.Default.WebsiteSelected = WebsiteSource.SelectedIndex;
            Settings.Default.Save();
            Logger.Log("Saved settings", "Main");
        }
        private void OpenInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log($"Opening in browser {OutputLabel.Content.ToString()}", "Main");
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = OutputLabel.Content.ToString() };
            System.Diagnostics.Process.Start(process);
            Logger.Log($"Opened in browser {OutputLabel.Content.ToString()}", "Main");
        }
        private void WebsiteSubSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.WebsiteSubSelected = WebsiteSubSelector.SelectedIndex;
            Settings.Default.Save();
        }
    }
}