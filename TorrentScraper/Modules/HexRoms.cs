using HtmlAgilityPack;
using SimpleThingsProvider.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleThingsProvider.Modules
{
    internal class HexRoms : IModule
    {
        public string Name { get { return "HexRom"; } set { } }
        public HtmlDocument Doc { get; set; }
        private List<string> _underlying;

        public List<string> getResults(HtmlDocument document, ListView ResultsList)
        {
            ResultsList.Visibility = Visibility.Visible;
            _underlying = new List<string>();
            List<Result> results = new();
            HtmlNodeCollection alist = document.DocumentNode.SelectNodes("/html/body/div[2]/div[1]/div/div[1]/div/div/ul/li/a");
            try
            {
                Logger.Log($"Found {alist.Count} results", "Websites (getResults - HexRom)");
                foreach (HtmlNode game in alist)
                {
                    try
                    {
                        results.Add(new Result() { Title = game.Attributes["title"].Value });
                        _underlying.Add(game.Attributes["href"].Value);
                    }
                    catch { continue; }
                }
            }
            catch (NullReferenceException)
            {
                Logger.Log("No results found!", "Websites (getResults - HexRom)");
                return new List<string>();
            }

            Logger.Log($"Found {_underlying.Count} entries", "Websites (getResults - HexRoms)");
            if (!Settings.Default.NSFWContent)
            {
                foreach (Result res in results)
                {
                    foreach (string s in BannedWords.nsfwWords)
                    {
                        if (res.Title.ToLower().Contains(s.ToLower()))
                        {
                            res.Title = "NSFW Content";
                            _underlying[results.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            ResultsList.ItemsSource = results;
            return _underlying;
        }

        public List<string> getResults(HtmlDocument document, ListView ResultsList, string toSearch){return null;}

        public string getLink(int index)
        {
            return getLink(_underlying[index] + "/download/");
        }

        public string getLink(string gameURL)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(gameURL);
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.HexRomsLinksList.Visibility = Visibility.Visible;
            linksWindow.Show();
            List<Result> websites = new List<Result>();
            var title = "";
            var link = "";
            //html/body/div[3]/div[1]/div/div/div/div[2]/div/table/tbody/tr/td/a
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("/html/body/div[2]/div[1]/div/div/div/div[2]/div/table/tbody/tr/td/a");
            System.Diagnostics.Debug.Write(links.Count);
            if (links == null) { return ""; }
            Logger.Log("Getting game page links", "Websites (getGamePage - HexRom)");
            foreach (HtmlNode downloadlink in links)
            {
                title = downloadlink.InnerText;
                link = downloadlink.Attributes["href"].Value;
                if (title != " ") { websites.Add(new Result() { Link = link, Infos = title }); }
            }
            Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - HexRom)");
            if (!Settings.Default.NSFWContent)
            {
                foreach (Result res in websites)
                {
                    foreach (string s in BannedWords.nsfwWords)
                    {
                        if (res.Infos.ToLower().Contains(s.ToLower()))
                        {
                            res.Infos = "NSFW Content";
                            _underlying[websites.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            linksWindow.HexRomsLinksList.ItemsSource = websites;
            return "";
        }

        public HttpStatusCode search(string toSearch)
        {
            HttpStatusCode code;
            HtmlWeb web = new();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            web.UserAgent = "SimpleThingsProvider";

            Logger.Log("Searching for: " + toSearch + " in: " + Name, "Website (Search)");
            try
            {
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://hexrom.com/roms/nintendo-3ds/?title=" + toSearch, Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://hexrom.com/roms/nintendo-3ds/?title=" + toSearch);
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound} for {mainWindow.WebsiteSource.SelectedItem.ToString()}", "Website (Search)"); return HttpStatusCode.NotFound; }

            code = web.StatusCode;
            return code;
        }
    }
}
