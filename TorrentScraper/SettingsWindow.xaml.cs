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
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Net.Http;
using ControlzEx.Theming;
using System.Diagnostics;

namespace SimpleThingsProvider
{
    /// <summary>
    /// Logica di interazione per ProxyWindow.xaml
    /// </summary>
    public partial class SettingsWindow
    {
        private List<IExtension> extensions;
        public SettingsWindow(List<IExtension> e)
        {
            InitializeComponent();
            extensions = e;
            if (Settings.Default.SyncWithWindows) { ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode; }
            else { ThemeManager.Current.ChangeTheme(this, Settings.Default.MainTheme + "." + Settings.Default.SubTheme); }

            ThemeManager.Current.SyncTheme();
            Logger.Log("Initialized Proxy Window", "Proxy");
            foreach (IExtension extension in extensions)
            {
                ExtensionsListView.Items.Add(extension.name);
                // Get all extension's settings
            }
            getSettings();
        }
        public void Alert(string messageBoxText, string caption)
        {
            Logger.Log(messageBoxText, "Alert");
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Information;
            MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.OK);
        }

        private void getSettings()
        {
            IPTextBox.Text = Settings.Default.ProxyIP;
            PortTextBox.Text = Settings.Default.ProxyPort;
            ProxyEnabledCheckBox.IsChecked = Settings.Default.ProxyEnabled;
            MainThemeComboBox.SelectedItem = Settings.Default.MainTheme;
            MainThemeComboBox.IsEnabled = true;
            SubThemeComboBox.SelectedItem = Settings.Default.SubTheme;
            SubThemeComboBox.IsEnabled = true;
            SyncWithWindowsCheckBox.IsChecked = Settings.Default.SyncWithWindows;
            NSFWContentCheckBox.IsChecked = Settings.Default.NSFWContent;
            if (Settings.Default.SyncWithWindows) { MainThemeComboBox.IsEnabled = false; SubThemeComboBox.IsEnabled = false; }
            Logger.Log($"Loaded previous settings for Proxy IP:{IPTextBox.Text} PORT:{PortTextBox.Text} ENABLED:{ProxyEnabledCheckBox.IsChecked}", "Proxy");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save STP's settings the old way
            Settings.Default.NSFWContent = NSFWContentCheckBox.IsChecked.Value;
            Logger.Log("Testing new proxy configuration", "Proxy");
            if (ProxyEnabledCheckBox.IsChecked.Value)
            {
                try
                {
                    HtmlWeb web = new HtmlWeb();
                    web.Load("https://google.com/", IPTextBox.Text, Int32.Parse(PortTextBox.Text), String.Empty, String.Empty);
                    Logger.Log("New proxy configuration is valid", "Proxy");
                }
                catch (Exception) { MainWindow mainWindow = (MainWindow)Application.Current.MainWindow; mainWindow.Alert("Proxy unreachable", "Error"); return; }
            }
            Settings.Default.ProxyPort = PortTextBox.Text.Replace(" ", "");
            Settings.Default.ProxyIP = IPTextBox.Text.Replace(" ", "");
            Settings.Default.ProxyEnabled = ProxyEnabledCheckBox.IsChecked.Value;
            if (Settings.Default.MainTheme != MainThemeComboBox.Text)
            {
                Settings.Default.MainTheme = MainThemeComboBox.Text;
                if (!Settings.Default.DoNotShowAgain_RestartApplicationAfterThemeChange)
                {
                    Alert("You need to restart the application to change the theme!", "Info");
                    Settings.Default.DoNotShowAgain_RestartApplicationAfterThemeChange = true;
                }
            }
            if (Settings.Default.SubTheme != SubThemeComboBox.Text)
            {
                Settings.Default.SubTheme = SubThemeComboBox.Text;
                if (!Settings.Default.DoNotShowAgain_RestartApplicationAfterThemeChange) {
                    Alert("You need to restart the application to change the theme!", "Info");
                    Settings.Default.DoNotShowAgain_RestartApplicationAfterThemeChange = true;
                }
            }
            Settings.Default.SyncWithWindows = SyncWithWindowsCheckBox.IsChecked.Value;
            if (Settings.Default.SyncWithWindows) { ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode; }
            else { ThemeManager.Current.ChangeTheme(this, Settings.Default.MainTheme + "." + Settings.Default.SubTheme); }
            ThemeManager.Current.SyncTheme();
            Settings.Default.Save();
            // Save the extensions's settings
            foreach (IExtension extension in extensions)
            {
                try
                {
                    extension.saveSettings();
                }
                catch(NullReferenceException ex)
                {
                    continue;
                }
                
            }
            Close();
        }

        private void extensionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ExtensionsListView.SelectedItem.Equals("Simple Things Provider"))
            {
                STPSettingsGrid.Visibility = Visibility.Visible;
                SettingsGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                STPSettingsGrid.Visibility = Visibility.Hidden;
                SettingsGrid.Visibility = Visibility.Visible;
            }
            // Load new grid got from the extension getSettings function
            foreach (IExtension extension in extensions)
            {
                if (extension.name.Equals(ExtensionsListView.SelectedItem))
                {
                    List<DockPanel> docks = extension.getSettings();
                    SettingsGridSP.Children.Clear();
                    foreach (DockPanel dock in docks)
                    {
                        SettingsGridSP.Children.Add(dock);
                    }
                }
            }
        }
    }
}