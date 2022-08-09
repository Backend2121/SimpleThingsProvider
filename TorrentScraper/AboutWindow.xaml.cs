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
        }
        private void openDiscord(object sender, RoutedEventArgs e)
        {
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://discord.com/invite/WTrCtvyPke" };
            System.Diagnostics.Process.Start(process);
        }
        private void openPatreon(object sender, RoutedEventArgs e)
        {
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://www.patreon.com/Backend2121" };
            System.Diagnostics.Process.Start(process);
        }
        private void openReddit(object sender, RoutedEventArgs e)
        {
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://www.reddit.com/user/Sbigioduro" };
            System.Diagnostics.Process.Start(process);
        }
        private void openYoutube(object sender, RoutedEventArgs e)
        {
            var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://www.youtube.com/channel/UCW2JQJs_R3O_I937yxicT9A" };
            System.Diagnostics.Process.Start(process);
        }
    }
}
