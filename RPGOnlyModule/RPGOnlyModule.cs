using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using HtmlAgilityPack;
using System.Windows.Data;

namespace SimpleThingsProvider
{
    internal class RPGOnlyModule : IModule
    {
        public string Name { get { return "RPGOnly"; } set { } }
        private List<string> _underlying;
        private string _toSearch;
        public HtmlDocument Doc { get; set; }
        public bool needsSubSelector { get { return false; } }
        public LinksWindow linksWindow { get { return new LinksWindow(); } }
        public RPGOnlyModule()
        {

        }
        public void buildListView(GridView grid)
        {
            GridViewColumn title = new GridViewColumn();
            title.Header = "Title";
            title.DisplayMemberBinding = new Binding("Title");
            grid.Columns.Add(title);
        }

        public HttpStatusCode search(string toSearch)
        {
            _toSearch = toSearch;
            HttpStatusCode code;
            HtmlWeb web = new();
            web.UserAgent = "SimpleThingsProvider";

            Logger.Log("Searching for: " + toSearch + " in: " + Name, "Website (Search)");

            try {
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://rpgonly.com/list-of-all-switch-games-nsp-xci/", Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://rpgonly.com/list-of-all-switch-games-nsp-xci/");
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound}", "Website (Search)"); return HttpStatusCode.NotFound; }

            return web.StatusCode;
        }
        public Tuple<List<Result>, List<string>> getResults(HtmlDocument document)
        {
            _underlying = new List<string>();
            List<Result> results = new();

            HtmlNodeCollection listNode = document.DocumentNode.SelectNodes("/html/body/div/div[6]/div/div[1]/div/main/article/div/div/ul/li");
            Logger.Log($"Found {listNode.Count} results", "Websites (getResults - RPGOnly)");
            try
            {
                foreach (HtmlNode node in listNode)
                {
                    IEnumerable<HtmlNode> descendants = node.Descendants(1);
                    foreach (HtmlNode descendant in descendants)
                    {
                        try
                        {
                            if (descendant.Attributes["href"].Value.Contains("https://"))
                            {
                                if (descendant.InnerText.ToLower().Contains(_toSearch.ToLower()))
                                {
                                    _underlying.Add(descendant.Attributes["href"].Value);
                                    results.Add(new Result() { Title = descendant.InnerText.Replace("&#038;", "&") });
                                }
                            }
                        }
                        catch (NullReferenceException) { continue; }
                    }
                }
            } 
            catch (NullReferenceException)
            {
                Logger.Log("No results found!", "Websites (getResults - RPGOnly)");
                return Tuple.Create(new List<Result>(), new List<string>());
            }
            Logger.Log($"Found {_underlying.Count} entries", "Websites (getResults - RPGOnly)");
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
        public string getLink(int index) {
            getLink(_underlying[index]);
            return null;
        }
        public string getLink(string gameURL)
        {
            HtmlWeb web = new();
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.getLinksList().Visibility = Visibility.Visible;
            linksWindow.Show();
            Doc = web.Load(gameURL);
            List<Result> websites = new List<Result>();
            HtmlNode entry = Doc.DocumentNode.SelectSingleNode("/html/body/div/div[6]/div/div[1]/div/main/div[1]/article/div/figure[2]/table/tbody");
            Logger.Log($"Getting game page links", "Websites (getGamePage - RPGOnly)");
            var infos = "";
            var name = "";
            var link = "";
            foreach (HtmlNode descendant in entry.ChildNodes)
            {
                //We have selected the trs
                bool firstTime = true;
                foreach (HtmlNode node in descendant.ChildNodes)
                {
                    //[0] childnode contains the only non-link text
                    if (firstTime)
                    {
                        firstTime = false;
                        name = node.InnerText;
                        continue;
                    }
                    try
                    {
                        foreach (HtmlNode node2 in node.ChildNodes)
                        {
                            infos = node2.InnerText;
                            link = node2.Attributes["href"].Value;
                            websites.Add(new Result() { Infos = infos, Name = name, Link = link });
                        }
                    }
                    catch { link = ""; continue; }
                }
            }
            Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - RPGOnly)");
            if (!Settings.Default.NSFWContent)
            {
                foreach (Result res in websites)
                {
                    foreach (string s in BannedWords.nsfwWords)
                    {
                        if (res.Name.ToLower().Contains(s.ToLower()))
                        {
                            res.Name = "NSFW Content";
                            _underlying[websites.IndexOf(res)] = string.Empty;
                        }
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
    }
}