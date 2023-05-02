using ControlzEx.Theming;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SimpleThingsProvider
{
    /// <summary>
    /// Logica di interazione per LinksWindow.xaml
    /// </summary>
    public partial class LinksWindow : Window
    {
        public LinksWindow linkWindow { get { return this; } }
        public LinksWindow()
        {
            InitializeComponent();
            if (Settings.Default.SyncWithWindows) { ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode; }
            else { ThemeManager.Current.ChangeTheme(this, Settings.Default.MainTheme + "." + Settings.Default.SubTheme); }
            ThemeManager.Current.SyncTheme();
            // Hide all linkslists
            LinksList.Visibility = Visibility.Visible;
            HexRomsLinksList.Visibility = Visibility.Hidden;
            Topmost = true;
        }
        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            string link = "";
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            if (LinksList.Visibility == Visibility.Visible)
            {
                link = ((Result)LinksList.SelectedValue).Link;
            }
            else if (HexRomsLinksList.Visibility == Visibility.Visible)
            {
                link = ((Result)HexRomsLinksList.SelectedValue).Link;
            }
            mainWindow.OutputLabel.Content = link;
            if (link != "") { Close(); }
        }
        public ListView getLinksList()
        {
            return LinksList;
        }
    }
}