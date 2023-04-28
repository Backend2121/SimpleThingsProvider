using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;
using Utils;

namespace SimpleThingsProvider.Modules
{
    class FitGirl : IModule
    {
        public string Name { get { return "FitGirl"; } set { } }
        public HtmlDocument Doc { get; set; }

        private List<string> _underlying;
        public ListView listview { get; set; }
        public bool needsSubSelector { get { return true; } }

        public FitGirl(ListView lv)
        {
            listview = lv;
        }

        public string getLink(int index)
        {
            return getLink(_underlying[index]);
        }

        public string getLink(string gameURL)
        {
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.Show();
            linksWindow.linkWindow.Visibility = Visibility.Visible;
            List<Result> websites = new List<Result>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(gameURL);
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("/html/body/div/div/div[1]/div/article/div/ul[1]/li/a");
            //Logger.Log($"Getting game page links", "Websites (getGamePage - FitGirl)");
            foreach (HtmlNode downloadlink in links)
            {
                websites.Add(new Result() { Link = downloadlink.Attributes["href"].Value, Infos = downloadlink.InnerText });
            }
            //Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - FitGirl)");
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
            linksWindow.linkWindow.ItemsSource = websites;
            return "";
        }

        public List<string> getResults(HtmlDocument document)
        {
            listview.Visibility = Visibility.Visible;
            _underlying = new List<string>();
            List<Result> results = new();

            HtmlNodeCollection games = document.DocumentNode.SelectNodes("/html/body/div/div/section/div/article/div/p");
            try
            {
                //Logger.Log($"Found {games.Count} results", "Websites (getResults - FitGirl)");
                foreach (HtmlNode game in games)
                {
                    string p = game.InnerText;
                    // Title section
                    var titleEnd = p.IndexOf("Genres/Tags:");
                    string title = p.Substring(0, titleEnd);
                    title = title.Replace("&#8217;", "’");
                    title = title.Replace("&#8211;", "-");
                    title = title.Replace("&#038;", "&");

                    // Size section
                    var sizeStart = p.IndexOf("Original Size:");
                    var sizeEnd = p.IndexOf("Repack Size:");
                    string size = p.Substring(sizeStart, sizeEnd - sizeStart);
                    size = size.Replace("Original Size:", "");


                    // repackSize section
                    var repacksizeStart = p.IndexOf("Repack Size:");
                    var repacksizeEnd = p.IndexOf("Download Mirrors");
                    string repacksize = p.Substring(repacksizeStart, repacksizeEnd - repacksizeStart);
                    repacksize = repacksize.Replace("Repack Size:", "");

                    //Link section
                    _underlying.Add(game.LastChild.Attributes["href"].Value);

                    results.Add(new Result() { Title = title, OriginalSize = size, RepackSize = repacksize });
                }
            }
            catch (NullReferenceException)
            {
                //Logger.Log("No results found!", "Websites (getResults - FitGirl)");
                return new List<string>();
            }

            //Logger.Log($"Found {_underlying.Count} entries", "Websites (getResults - FitGirl)");
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
            listview.ItemsSource = results;
            return _underlying;
        }
        public HttpStatusCode search(string toSearch)
        {
            HttpStatusCode code;
            HtmlWeb web = new();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            web.UserAgent = "SimpleThingsProvider";

           //Logger.Log("Searching for: " + toSearch + " in: " + Name, "Website (Search)");
            try
            {
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://fitgirl-repacks.site/?s=" + toSearch.Replace(" ", "+"), Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://fitgirl-repacks.site/?s=" + toSearch.Replace(" ", "+"));
            }
            catch { 
                //Logger.Log($"Error code {HttpStatusCode.NotFound} for {mainWindow.WebsiteSource.SelectedItem.ToString()}", "Website (Search)");
                return HttpStatusCode.NotFound;
            }

            code = web.StatusCode;
            return code;
        }
    }
}
