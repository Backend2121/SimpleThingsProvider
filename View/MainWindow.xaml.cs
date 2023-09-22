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
using Utils;

namespace SimpleThingsProvider
{
    public partial class MainWindow
    {
        List<string> underlying;
        List<IModule> ImodulesList;
        IModule module;
        Random r = new Random();
        string[] motd = {"Hi!", "Hello there!", "Hey!", "Honk!", "Whassup!", "I promise i won't hang", "What do you need?", "Here to help!", "How are you doing?", "Join the Discord!", "Praise the Sun!", "For science, you monster", "It's dangerous to go alone, use me!", "Stupid Shinigami", "Trust me i'm a dolphin!", "...", "Oh, it's you...", "The cake is a lie!", "FBI open up!", "You own the game, right?"};
        public MainWindow()
        {
            InitializeComponent();
            Logger.Log("Loading modules", "Main");
            ImodulesList = new List<IModule>();
            loadModules();
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
            catch(Exception ex) { Logger.Log(ex.ToString(), "Main"); }
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
            // Here temporarly need to auto-import them

            /*IModule x1337 = new Modules.x1337(TorrentResultsList);
            IModule thePirateBay = new Modules.ThePirateBay(TorrentResultsList);
            IModule rpgOnly = new Modules.RPGOnly(RPGOnlyResultsList);
            IModule ziperto = new Modules.Ziperto(ZipertoResultsList);
            IModule hexRoms = new Modules.HexRoms(HexRomResultsList);
            IModule fitGirl = new Modules.FitGirl(FitGirlResultsList);
            IModule vimmsLair = new Modules.Vimmslair(VimmResultsList);
            ImodulesList.Add(x1337);
            ImodulesList.Add(thePirateBay);
            ImodulesList.Add(rpgOnly);
            ImodulesList.Add(ziperto);
            ImodulesList.Add(hexRoms);
            ImodulesList.Add(fitGirl);
            ImodulesList.Add(vimmsLair);*/
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
        private void Search(object sender, RoutedEventArgs e)
        {
            // Reset to default
            OpenInBrowserButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            OutputLabel.Content = motd[r.Next(0, motd.Length - 1)];

            // Hide all ResultsLists
            TorrentResultsList.Visibility = Visibility.Hidden;
            VimmResultsList.Visibility = Visibility.Hidden;
            FitGirlResultsList.Visibility = Visibility.Hidden;
            NxBrewResultsList.Visibility = Visibility.Hidden;
            ZipertoResultsList.Visibility = Visibility.Hidden;
            WowRomsResultsList.Visibility = Visibility.Hidden;
            RPGOnlyResultsList.Visibility = Visibility.Hidden;
            HexRomResultsList.Visibility = Visibility.Hidden;
            MangaFreakResultsList.Visibility = Visibility.Hidden;
            MangaWorldResultsList.Visibility = Visibility.Hidden;
            foreach (IModule m in ImodulesList)
            {
                if (m.Name == WebsiteSource.Text)
                {
                    module = m;
                }
            }

            HttpStatusCode code = module.search(SearchTextBox.Text);
           
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
            
            if (code != HttpStatusCode.OK) 
            {
                Alert("Received a non 200(OK) response!" + "\n" + code, "STP: Error");
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
                        underlying = m.getResults(module.Doc);
                    }
                }
                StatusCodeLabel.Content = "Status Code: " + code;
                StatusCodeLabel.Foreground = new SolidColorBrush(Colors.Green);
            }
            if (underlying.Count <= 0)
            {
                Alert("No results found!", "STP: Warning");
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
            HtmlWeb web = new HtmlWeb();
            try
            {
                string entry = "";
                entry = module.getLink(module.listview.SelectedIndex);
                Logger.Log($"Entry {entry} has been selected", "Main");
                OutputLabel.Content = entry;
                CopyButton.IsEnabled = true;
                OpenInBrowserButton.IsEnabled = true;
                return;
            }
            catch (ArgumentOutOfRangeException) { CopyButton.IsEnabled = true; OpenInBrowserButton.IsEnabled = false; OutputLabel.Content = "Error "; return; }
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
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Show();
            settingsWindow.Focus();
        }
        private void SaveSelected(object sender, RoutedEventArgs e)
        {
            // Enables subselector for the next time it changes
            Debug.WriteLine(WebsiteSource.Text);
            Debug.WriteLine((sender as ComboBox).SelectedItem.ToString());
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
    }
}