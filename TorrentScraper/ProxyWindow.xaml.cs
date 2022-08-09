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

namespace SimpleThingsProvider
{
    /// <summary>
    /// Logica di interazione per ProxyWindow.xaml
    /// </summary>
    public partial class ProxyWindow : Window
    {
        public ProxyWindow()
        {
            InitializeComponent();
            getSettings();
        }

        private void getSettings()
        {
            IPTextBox.Text = Settings.Default.ProxyIP;
            PortTextBox.Text = Settings.Default.ProxyPort;
            ProxyEnabledCheckBox.IsChecked = Settings.Default.ProxyEnabled;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProxyEnabledCheckBox.IsChecked.Value)
            {
                try
                {
                    HtmlWeb web = new HtmlWeb();
                    web.Load("https://google.com/", IPTextBox.Text, Int32.Parse(PortTextBox.Text), String.Empty, String.Empty);
                }
                catch (Exception) { MainWindow mainWindow = (MainWindow)Application.Current.MainWindow; mainWindow.Alert("Proxy unreachable", "Error"); return; }
            }
            

            Settings.Default.ProxyPort = PortTextBox.Text;
            Settings.Default.ProxyIP = IPTextBox.Text;
            Settings.Default.ProxyEnabled = ProxyEnabledCheckBox.IsChecked.Value;
            Settings.Default.Save();
            Close();
        }
    }
}
