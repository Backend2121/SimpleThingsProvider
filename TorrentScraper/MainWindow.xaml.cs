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
using System.Diagnostics;
using MahApps.Metro.Controls;
using ControlzEx.Theming;
using MahApps;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Reflection.Metadata;
using ControlzEx.Standard;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SimpleThingsProvider
{
    public partial class MainWindow
    {
        List<string> underlying;
        List<IModule> ImodulesList;
        List<IExtension> IextensionsList;
        List<Result> results = new();
        HttpStatusCode code = new HttpStatusCode();
        GridView grid;
        IModule module;
        Random r = new Random();
        string[] motd = { "Hi!", "Hello there!", "Hey!", "Honk!", "Whassup!", "I promise i won't hang", "What do you need?", "Here to help!", "How are you doing?", "Join the Discord!", "Praise the Sun!", "For science, you monster", "It's dangerous to go alone, use me!", "Stupid Shinigami", "Trust me i'm a dolphin!", "...", "Oh, it's you...", "The cake is a lie!", "FBI open up!", "You own the game, right?" };
        private bool extensionsMenu_FIX = false;
        public MainWindow()
        {
            InitializeComponent();
            Logger.Log("Loading modules", "Main");
            ImodulesList = new List<IModule>();
            IextensionsList = new List<IExtension>();
            loadModules();
            StatusCodeLabel.Foreground = new SolidColorBrush(Colors.DarkGoldenrod);
            if (Settings.Default.SyncWithWindows) { ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode; }
            else { ThemeManager.Current.ChangeTheme(this, Settings.Default.MainTheme + "." + Settings.Default.SubTheme); }

            ThemeManager.Current.SyncTheme();
            Application.Current.MainWindow = this;
            this.SizeToContent = SizeToContent.WidthAndHeight;

            Logger.Log("Initialized MainWindow", "Main");

            WebsiteSource.SelectedIndex = Settings.Default.WebsiteSelected;
            WebsiteSubSelector.SelectedIndex = Settings.Default.WebsiteSubSelected;

            Logger.Log($"Loaded settings: {WebsiteSource.SelectedIndex}___{WebsiteSubSelector.SelectedIndex}", "Main");
            try
            {
                foreach (IModule m in ImodulesList)
                {
                    if (m.Name.Equals(WebsiteSource.SelectedItem.ToString()))
                    {
                        if (m.needsSubSelector) { WebsiteSubSelector.IsEnabled = true; }
                        else { WebsiteSubSelector.IsEnabled = false; }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex.ToString(), "Main"); }
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
            PiracyDisclaimer();
            OutputLabel.Content = motd[r.Next(0, motd.Length - 1)];
        }
        private void loadModules()
        {
            Assembly dll;
            Type dllType;
            // Load all dlls found inside the "Modules" folder
            string[] modules = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Modules\\");
            WebsiteSource.Items.Clear();
            foreach (string module in modules)
            {
                dll = Assembly.LoadFrom(module);
                dllType = dll.GetType("SimpleThingsProvider." + module.Substring(module.LastIndexOf("\\") + 1, module.Length - module.LastIndexOf("\\") - 5));
                IModule m = (IModule)Activator.CreateInstance(dllType, new Object[] { });
                ImodulesList.Add(m);
                WebsiteSource.Items.Add(m.Name);
                m.checkUpdate();
            }
            string[] extensions = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Extensions\\");
            foreach (string extension in extensions)
            {
                if (extension.Contains("Extension.dll"))
                {
                    dll = Assembly.LoadFrom(extension);
                    dllType = dll.GetType("SimpleThingsProvider." + extension.Substring(extension.LastIndexOf("\\") + 1, extension.Length - extension.LastIndexOf("\\") - 5));
                    IExtension e = (IExtension)Activator.CreateInstance(dllType, new Object[] { });
                    IextensionsList.Add(e);
                    addUIElements(e);
                    ExtensionsMenu.Items.Add(e.Name);
                    e.disableButton(OutputLabel);
                    e.checkUpdate();
                }
            }
        }
        private void addUIElements(IExtension extension)
        {
            // Check available spaces for the SINGLE button an extension can provide, if any
            List<UIElement> uIElements = new List<UIElement>();
            uIElements = extension.getElements(ResultsList, OutputLabel);
            if (uIElements.Count > 0)
            {
                Button eButton = (Button)uIElements[0];
                int currentColumn = 0;
                int currentRow = 0;
                if (eButton != null)
                {
                    Grid.SetColumn(eButton, currentColumn++);
                    Grid.SetRow(eButton, currentRow);
                    if (currentColumn <= 0)
                    {
                        currentColumn = 2;
                        currentRow++;
                    }
                    ButtonGrid.Children.Add(eButton);
                }
            }
            else
            {
                Logger.Log("No button provided by " + extension.Name, "Main");
            }
        }
        private void checkUpdate()
        {
            string repoURL = "https://raw.githubusercontent.com/Backend2121/SimpleThingsProvider/master/TorrentScraper/Settings.settings";

            WebClient webClient = new WebClient();
            Stream stream = webClient.OpenRead(repoURL);
            StreamReader reader = new StreamReader(stream);
            string content = reader.ReadToEnd();
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
        private void Search(object sender, RoutedEventArgs e)
        {
            // Reset to default
            OpenInBrowserButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            foreach (IExtension ex in IextensionsList)
            {
                ex.disableButton(OutputLabel);
            }
            OutputLabel.Content = motd[r.Next(0, motd.Length - 1)];
            StatusCodeLabel.Content = "Status Code: ";
            StatusCodeLabel.Foreground = new SolidColorBrush(Colors.DarkGoldenrod);
            results.Clear();

            foreach (IModule m in ImodulesList)
            {
                if (m.Name == WebsiteSource.Text)
                {
                    module = m;
                }
            }
            if (!Settings.Default.NSFWContent)
            {
                foreach (string s in BannedWords.nsfwWords)
                {
                    if (SearchTextBox.Text.Contains(s.ToLower()))
                    {
                        Alert("NSFW content detected!", "STP: Warning");
                        ResultsNumber.Content = "Results: 0";
                        return;
                    }
                }
            }
            // Add background check
            BackgroundWorker statusCodeWorker = new BackgroundWorker();
            statusCodeWorker.DoWork += Worker_StatusCode;
            statusCodeWorker.RunWorkerCompleted += Worker_StatusCodeCompleted;
            statusCodeWorker.RunWorkerAsync(SearchTextBox.Text);
            // Loading gif
            LoadingGif.Visibility = Visibility.Visible;
        }
        private void Worker_Search(object? sender, DoWorkEventArgs e)
        {
            (results, underlying) = ((IModule)e.Argument).getResults(module.Doc);
        }
        private void Worker_SearchCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (underlying.Count <= 0)
            {
                Alert("No results found!", "STP: Warning");
                ResultsNumber.Content = "Results: 0";
                return;
            }
            else { ResultsNumber.Content = "Results: " + underlying.Count; }
            getResultsList().View = grid;
            getResultsList().ItemsSource = results;
            getResultsList().Visibility = Visibility.Visible;
            LoadingGif.Visibility = Visibility.Hidden;
        }
        private void Worker_StatusCode(object? sender, DoWorkEventArgs e)
        {
            code = module.search(e.Argument.ToString());
        }
        private void Worker_StatusCodeCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (code != HttpStatusCode.OK)
            {
                //Alert("Received a non 200(OK) response!" + "\n" + code, "STP: Error");
                StatusCodeLabel.Content = "Status Code: " + code;
                StatusCodeLabel.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            else
            {
                foreach (IModule m in ImodulesList)
                {
                    if (m.Name.Equals(WebsiteSource.Text))
                    {
                        // Start async thread to fetch results
                        grid = new GridView();
                        m.buildListView(grid);
                        BackgroundWorker searchWorker = new BackgroundWorker();
                        searchWorker.DoWork += Worker_Search;
                        searchWorker.RunWorkerCompleted += Worker_SearchCompleted;
                        searchWorker.RunWorkerAsync(m);
                    }
                }
                StatusCodeLabel.Content = "Status Code: " + code;
                StatusCodeLabel.Foreground = new SolidColorBrush(Colors.Green);
            }
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

            return MessageBox.Show("A new version of this software has been found, do you want to open the download page?", "UPDATE", button, icon, MessageBoxResult.Yes);
        }
        private void PiracyDisclaimer()
        {
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;

            MessageBox.Show("This software MUST NOT be used to illegally obtain any form of media!\n\nI do not condone any form of piracy!", "WARNING", button, icon, MessageBoxResult.Yes);
        }
        private void ResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string entry = "";
                if (ResultsList.SelectedIndex != null)
                {
                    entry = module.getLink(ResultsList.SelectedIndex);
                    Logger.Log($"Entry {entry} has been selected", "Main");
                    OutputLabel.Content = entry;
                    CopyButton.IsEnabled = true;
                    OpenInBrowserButton.IsEnabled = true;
                    foreach (IExtension ex in IextensionsList)
                    {
                        ex.enableButton(OutputLabel);
                    }
                }
                return;
            }
            catch (ArgumentOutOfRangeException)
            {
                CopyButton.IsEnabled = true;
                OpenInBrowserButton.IsEnabled = false;
                foreach (IExtension ex in IextensionsList)
                {
                    ex.disableButton(OutputLabel);
                }
                OutputLabel.Content = "Error ";
                return;
            }
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
        private void OpenHelpWindow(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
            helpWindow.Focus();
        }
        private void OpenAboutWindow(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
            aboutWindow.Focus();
        }
        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(IextensionsList);
            settingsWindow.Show();
            settingsWindow.Focus();
        }
        /*private void OpenDownloader(object sender, RoutedEventArgs e)
        {
            foreach (IExtension extension in IextensionsList)
            {
                if (extension.Name == "Downloader")
                {
                    var w = extension.getExtensionWindow();
                    w.Show();
                    w.Focus();
                    object[] args = Array.Empty<object>();
                    extension.startFunction(args);
                }
            }
        }*/
        private void SaveSelected(object sender, RoutedEventArgs e)
        {
            // Enables subselector for the next time it changes
            Logger.Log("Saving settings", "Main");
            foreach(IModule m in ImodulesList)
            {
                if (m.Name.Equals((sender as ComboBox).SelectedItem.ToString()))
                {
                    if (m.needsSubSelector) { WebsiteSubSelector.IsEnabled = true; }
                    else { WebsiteSubSelector.IsEnabled = false; }
                }
            }
            Settings.Default.WebsiteSelected = WebsiteSource.SelectedIndex;
            Settings.Default.Save();
            Logger.Log("Saved settings", "Main");
        }
        private void OpenInBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log($"Opening in browser {OutputLabel.Content.ToString()}", "Main");
            var patreon = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://Patreon.com/Backend2121" };
            System.Diagnostics.Process.Start(patreon);
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = OutputLabel.Content.ToString() };
            System.Diagnostics.Process.Start(process);
            Logger.Log($"Opened in browser {OutputLabel.Content.ToString()}", "Main");
        }
        private void WebsiteSubSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.WebsiteSubSelected = WebsiteSubSelector.SelectedIndex;
            Settings.Default.Save();
        }
        public ComboBox getWebsiteSource()
        {
            return WebsiteSource;
        }
        public List<IExtension> getExtensions()
        {
            return IextensionsList;
        }
        public ListView getResultsList()
        {
            return ResultsList;
        }
        private void ExtensionsMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!extensionsMenu_FIX)
            {
                extensionsMenu_FIX = true;
            }
            else
            {
                // Iterate through extensions list and find the selected one, then open it's corresponding window
                foreach (IExtension ex in IextensionsList)
                {
                    if (ex.Name.Equals(ExtensionsMenu.SelectedItem))
                    {
                        ex.showWindow();
                        if (Settings.Default.SyncWithWindows) { ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode; }
                        else { ThemeManager.Current.ChangeTheme(ex.extensionWindow, Settings.Default.MainTheme + "." + Settings.Default.SubTheme); }

                        ThemeManager.Current.SyncTheme();
                    }
                }
            }
        }
        public void ExtensionsMenu_Click(object sender, RoutedEventArgs e)
        {
            foreach (IExtension ex in IextensionsList)
            {
                if (ex.Name.Equals(ExtensionsMenu.SelectedItem))
                {
                    ex.showWindow();
                }
            }
        }
    }
}