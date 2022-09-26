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
    /// Logica di interazione per LinksWindow.xaml
    /// </summary>
    public partial class LinksWindow : Window
    {
        public LinksWindow()
        {
            InitializeComponent();
            if (Settings.Default.SyncWithWindows) { ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode; }
            else { ThemeManager.Current.ChangeTheme(this, Settings.Default.MainTheme + "." + Settings.Default.SubTheme); }

            ThemeManager.Current.SyncTheme();
            // Hide all linkslists
            LinksList.Visibility = Visibility.Hidden;
            HexRomsLinksList.Visibility = Visibility.Hidden;
            Topmost = true;
        }
        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            string link = "";
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (LinksList.Visibility == Visibility.Visible)
            {
                link = ((Websites.GameWebsite)LinksList.SelectedValue).Link;
            }
            else if (HexRomsLinksList.Visibility == Visibility.Visible)
            {
                link = ((Websites.HexRomsGameWebsite)HexRomsLinksList.SelectedValue).Link;
            }
            mainWindow.OutputLabel.Content = link;
            if (link != "") { Close(); }
        }
    }
}