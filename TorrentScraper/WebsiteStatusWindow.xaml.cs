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
using System.Net.Http;
using ControlzEx.Theming;

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
                            Logger.Log($"Website {x1337.name} answered with {x1337.code} code", "WebsiteStatus");
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website x1337 = new Website() { name = "x1337", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(x1337);
                            Logger.Log($"Website {x1337.name} answered with {x1337.code} code, {err}", "WebsiteStatus");
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
                            Logger.Log($"Website {thepiratebay.name} answered with {thepiratebay.code} code", "WebsiteStatus");
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website thepiratebay = new Website() { name = "ThePirateBay", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(thepiratebay);
                            Logger.Log($"Website {thepiratebay.name} answered with {thepiratebay.code} code, {err}", "WebsiteStatus");
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
                            Logger.Log($"Website {rpgonly.name} answered with {rpgonly.code} code", "WebsiteStatus");
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website rpgonly = new Website() { name = "RPGOnly", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(rpgonly);
                            Logger.Log($"Website {rpgonly.name} answered with {rpgonly.code} code,  {err}", "WebsiteStatus");
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
                            Logger.Log($"Website {nxbrew.name} answered with {nxbrew.code} code", "WebsiteStatus");
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website nxbrew = new Website() { name = "NxBrew", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(nxbrew);
                            Logger.Log($"Website {nxbrew.name} answered with {nxbrew.code} code,  {err}", "WebsiteStatus");
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
                            Logger.Log($"Website {hexrom.name} answered with {hexrom.code} code", "WebsiteStatus");
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website hexrom = new Website() { name = "HexRom", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(hexrom);
                            Logger.Log($"Website {hexrom.name} answered with {hexrom.code} code,  {err}", "WebsiteStatus");
                        });
                        break;
                    }
                case "FitGirl":
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website fitgirl = new Website() { name = "FitGirl", code = getStatus("https://fitgirl-repacks.site/") };
                            WebsiteList.Items.Add(fitgirl);
                            Logger.Log($"Website {fitgirl.name} answered with {fitgirl.code} code", "WebsiteStatus");
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website fitgirl = new Website() { name = "FitGirl", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(fitgirl);
                            Logger.Log($"Website {fitgirl.name} answered with {fitgirl.code} code,   {err}", "WebsiteStatus");
                        });
                        break;
                    }
                case "VimmsLair":
                    try
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website vimm = new Website() { name = "VimmsLair", code = getStatus("https://vimm.net/?p=vault") };
                            WebsiteList.Items.Add(vimm);
                            Logger.Log($"Website {vimm.name} answered with {vimm.code} code", "WebsiteStatus");
                        });
                        break;
                    }
                    catch (System.Net.WebException err)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            Website vimm = new Website() { name = "VimmsLair", code = HttpStatusCode.ServiceUnavailable, info = "Website is down or unreachable without a VPN or Proxy" };
                            WebsiteList.Items.Add(vimm);
                            Logger.Log($"Website {vimm.name} answered with {vimm.code} code, {err}", "WebsiteStatus");
                        });
                        break;
                    }
            }   
        }
        public WebsiteStatusWindow()
        {
            InitializeComponent();
            if (Settings.Default.SyncWithWindows) { ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode; }
            else { ThemeManager.Current.ChangeTheme(this, Settings.Default.MainTheme + "." + Settings.Default.SubTheme); }

            ThemeManager.Current.SyncTheme();
            Logger.Log("Initialized Website Status Window", "WebsiteStatus");
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            Title = "Website Status Checker (I'm not frozen!)";
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
                int port = Int32.Parse(Settings.Default.ProxyPort);
                web.Load(url, Settings.Default.ProxyIP, port, String.Empty, String.Empty);
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