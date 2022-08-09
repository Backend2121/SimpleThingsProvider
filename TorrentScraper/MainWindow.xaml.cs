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
            WebsiteSource.SelectedIndex = Settings.Default.WebsiteSelected;
            WebsiteSubSelector.SelectedIndex = Settings.Default.WebsiteSubSelected;
            if (WebsiteSource.SelectedItem.ToString() == "WoWRoms") { WebsiteSubSelector.IsEnabled = true; }
            else { WebsiteSubSelector.IsEnabled = false; }
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
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxResult result;

            result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
        }
        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HtmlWeb web = new HtmlWeb();
            try
            {
                var magnet = module.getMagnet(ResultsList.SelectedIndex);
                OutputLabel.Content = magnet;
                CopyButton.IsEnabled = true;
                OpenInBrowserButton.IsEnabled = true;
                return;
            }
            catch (ArgumentOutOfRangeException) { CopyButton.IsEnabled = true; OpenInBrowserButton.IsEnabled = false; OutputLabel.Content = "Output"; return; }
        }
        private void Copy(object sender, RoutedEventArgs e)
        {
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
            if (WebsiteSource.SelectedItem.ToString() == "WoWRoms") { WebsiteSubSelector.IsEnabled = true; }
            else { WebsiteSubSelector.IsEnabled = false; }
            Settings.Default.WebsiteSelected = WebsiteSource.SelectedIndex;
            Settings.Default.Save();
        }
        private void OpenInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = OutputLabel.Content.ToString() };
            System.Diagnostics.Process.Start(process);
        }
        private void WebsiteSubSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.WebsiteSubSelected = WebsiteSubSelector.SelectedIndex;
            Settings.Default.Save();
        }
    }
}