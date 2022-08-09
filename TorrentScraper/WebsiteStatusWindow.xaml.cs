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
using System.Net;
using HtmlAgilityPack;
using System.Threading;
using System.ComponentModel;

namespace SimpleThingsProvider
{
    /// <summary>
    /// Logica di interazione per WebsiteStatusWindow.xaml
    /// </summary>
    public partial class WebsiteStatusWindow : Window
    {
        public class Website
        {
            public string name { get; set; }
            public HttpStatusCode code { get; set; }
            public string info { get; set; }
        }

        void checkWebsite(object sender, DoWorkEventArgs e)
        {
            switch (e.Argument)
            {
                case "x1337":
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website x1337 = new Website() { name = "x1337", code = getStatus("https://www.1377x.to/") };
                            WebsiteList.Items.Add(x1337);
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website rpgonly = new Website() { name = "x1337", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(rpgonly);
                        });
                        break;
                    }
                case "ThePirateBay":
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website thepiratebay = new Website() { name = "ThePirateBay", code = getStatus("https://thepiratebay3.co/") };
                            WebsiteList.Items.Add(thepiratebay);
                        });
                        break;
                    }
                    catch (System.Net.WebException err) 
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website rpgonly = new Website() { name = "ThePirateBay", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(rpgonly);
                        });
                        break;
                    }
                case "RPGOnly":
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website rpgonly = new Website() { name = "RPGOnly", code = getStatus("https://rpgonly.com/") };
                            WebsiteList.Items.Add(rpgonly);
                        });
                        break;
                    }
                    catch (System.Net.WebException err) 
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website rpgonly = new Website() { name = "RPGOnly", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(rpgonly);
                        });
                        break; 
                    }
                case "NxBrew":
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website nxbrew = new Website() { name = "NxBrew", code = getStatus("https://nxbrew.com/") };
                            WebsiteList.Items.Add(nxbrew);
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website nxbrew = new Website() { name = "NxBrew", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(nxbrew);
                        });
                        break;
                    }
                case "HexRom":
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website hexrom = new Website() { name = "HexRom", code = getStatus("https://hexrom.com/") };
                            WebsiteList.Items.Add(hexrom);
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website hexrom = new Website() { name = "HexRom", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(hexrom);
                        });
                        break;
                    }
                }
            }
            public WebsiteStatusWindow()
        {
            InitializeComponent();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            foreach (var item in mainWindow.WebsiteSource.Items)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(checkWebsite);
                worker.RunWorkerAsync(item.ToString());
            }
        }
        private HttpStatusCode getStatus(string url)
        {
            HtmlWeb web = new HtmlWeb();
            if (Settings.Default.ProxyEnabled)
            {
                string proxy = Settings.Default.ProxyIP;
                int port = Int32.Parse(Settings.Default.ProxyPort);
                web.Load(url, proxy, port, String.Empty, String.Empty);
                return web.StatusCode;
            }
            else
            {
                web.Load(url);
                return web.StatusCode;
            }
            
        }
    }
}