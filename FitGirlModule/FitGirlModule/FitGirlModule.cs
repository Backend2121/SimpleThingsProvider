using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HtmlAgilityPack;

namespace SimpleThingsProvider
{
    class FitGirlModule : IModule
    {
        public string Name { get { return "FitGirl"; } set { } }
        public string ModuleVersion { get { return "1.0.0"; } set { } }
        public HtmlDocument Doc { get; set; }
        private List<string> _underlying;
        public bool needsSubSelector { get { return false; } }
        public LinksWindow linksWindow { get { return new LinksWindow(); } }
        public FitGirlModule()
        {

        }
        public void buildListView(GridView grid)
        {
            GridViewColumn title = new GridViewColumn();
            title.Header = "Title";
            title.DisplayMemberBinding = new Binding("Title");
            GridViewColumn size = new GridViewColumn();
            size.Header = "Repack Size";
            size.DisplayMemberBinding = new Binding("RepackSize");
            GridViewColumn repack = new GridViewColumn();
            repack.Header = "Original Size";
            repack.DisplayMemberBinding = new Binding("OriginalSize");
            grid.Columns.Add(title);
            grid.Columns.Add(size);
            grid.Columns.Add(repack);
        }
        public string getLink(int index)
        {
            return getLink(_underlying[index]);
        }

        public string getLink(string gameURL)
        {
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.Show();
            linksWindow.getLinksList().Visibility = Visibility.Visible;
            List<Result> websites = new List<Result>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(gameURL);
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("/html/body/div/div/div[1]/div/article/div/ul[1]/li/a");
            Logger.Log($"Getting game page links", "Websites (getGamePage - FitGirl)");
            foreach (HtmlNode downloadlink in links)
            {
                websites.Add(new Result() { Link = downloadlink.Attributes["href"].Value, Infos = downloadlink.InnerText });
            }
            Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - FitGirl)");
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
            linksWindow.getLinksList().ItemsSource = websites;
            return "";
        }

        public Tuple<List<Result>, List<string>> getResults(HtmlDocument document)
        {
            _underlying = new List<string>();
            List<Result> results = new();

            HtmlNodeCollection games = document.DocumentNode.SelectNodes("/html/body/div/div/section/div/article/div/p");
            try
            {
                Logger.Log($"Found {games.Count} results", "Websites (getResults - FitGirl)");
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
                Logger.Log("No results found!", "Websites (getResults - FitGirl)");
                return Tuple.Create(new List<Result>(), new List<string>());
            }

            Logger.Log($"Found {_underlying.Count} entries", "Websites (getResults - FitGirl)");
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
            return Tuple.Create(results, _underlying);
        }
        public HttpStatusCode search(string toSearch)
        {
            HttpStatusCode code;
            HtmlWeb web = new();
            web.UserAgent = "SimpleThingsProvider";

            Logger.Log("Searching for: " + toSearch + " in: " + Name, "Website (Search)");
            try
            {
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://fitgirl-repacks.site/?s=" + toSearch.Replace(" ", "+"), Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://fitgirl-repacks.site/?s=" + toSearch.Replace(" ", "+"));
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }

            code = web.StatusCode;
            return code;
        }
        public async void checkUpdate()
        {
            string repoURL = "https://raw.githubusercontent.com/Backend2121/SimpleThingsProvider/master/FitGirlModule/FitGirlModule/Info.json";
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, repoURL);
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("User-Agent", "SimpleThingsProvider"));
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string content = await response.Content.ReadAsStringAsync();
            int start = content.IndexOf(": \"");
            int end = content.IndexOf('"', start + 3);
            string version = content.Substring(start + 3, end - start - 3);
            if (!version.Equals(ModuleVersion))
            {
                MessageBoxResult r = AlertClass.Alert("An update for " + Name + " is available, open the GitHub page?", Name, MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (r == MessageBoxResult.Yes)
                {
                    var process = new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "https://github.com/Backend2121/SimpleThingsProvider/releases/latest" };
                    System.Diagnostics.Process.Start(process);
                    Logger.Log($"Newer version for {Name}: {version} found!", Name + " Updater");
                }
            }
        }
    }
}
