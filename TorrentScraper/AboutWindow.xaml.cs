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
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            VersionLabel.Content = Settings.Default.ApplicationVersion;
            Logger.Log("About Window initialized", "About");
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
    }
}
