using ControlzEx.Theming;
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

namespace SimpleThingsProvider
{
    /// <summary>
    /// Logica di interazione per AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        private byte[] egg = { 54, 56, 55, 52, 55, 52, 55, 48, 55, 51, 51, 97, 50, 102, 50, 102, 55, 57, 54, 102, 55, 53, 55, 52, 55, 53, 50, 101, 54, 50, 54, 53, 50, 102, 54, 102, 54, 53, 51, 49, 52, 97, 52, 102, 55, 97, 53, 51, 52, 99, 55, 53, 52, 97, 54, 51 };
        public AboutWindow()
        {
            InitializeComponent();
            if (Settings.Default.SyncWithWindows) { ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode; }
            else { ThemeManager.Current.ChangeTheme(this, Settings.Default.MainTheme + "." + Settings.Default.SubTheme); }

            ThemeManager.Current.SyncTheme();
            VersionLabel.Content = Settings.Default.ApplicationVersion;
            Logger.Log("About Window initialized", "About");
            this.SizeToContent = SizeToContent.WidthAndHeight;
        }
        public static string _2121(String h, Encoding e)
        {
            System.Diagnostics.Debug.WriteLine(h);
            int n = h.Length;
            byte[] b = new byte[n / 2];
            for (int i = 0; i < n; i += 2)
            {
                b[i / 2] = Convert.ToByte(h.Substring(i, 2), 16);
            }
            return e.GetString(b);
        }
        private void openDiscord(object sender, RoutedEventArgs e)
        {
            Logger.Log("Opening Discord Invite", "About");
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://discord.com/invite/WTrCtvyPke" };
            System.Diagnostics.Process.Start(process);
            Logger.Log("Opened Discord Invite", "About");
        }
        private void openPatreon(object sender, RoutedEventArgs e)
        {
            Logger.Log("Opening Patreon Page", "About");
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://www.patreon.com/Backend2121" };
            System.Diagnostics.Process.Start(process);
            Logger.Log("Opened Patreon Page", "About");
        }
        private void openReddit(object sender, RoutedEventArgs e)
        {
            Logger.Log("Opening Reddit Profile", "About");
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://www.reddit.com/user/Sbigioduro" };
            System.Diagnostics.Process.Start(process);
            Logger.Log("Opened Reddit Profile", "About");
        }
        private void openYoutube(object sender, RoutedEventArgs e)
        {
            Logger.Log("Opening Youtube Channel", "About");
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://www.youtube.com/channel/UCW2JQJs_R3O_I937yxicT9A" };
            System.Diagnostics.Process.Start(process);
            Logger.Log("Opened Youtube Channel", "About");
        }
        private void openEE(object sender, RoutedEventArgs e)
        {
            // That random textbox is not useless!
            Logger.Log("Testing the Super Secret Code Box", "About");
            if (SuperSecretCodeTextBox.Text == "HelloThereGeneralKenobi")
            {
                Logger.Log("* got in!", "About");
                var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = _2121(Encoding.ASCII.GetString(egg), Encoding.ASCII) };
                System.Diagnostics.Process.Start(process);
            }
        }
    }
}