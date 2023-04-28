using HtmlAgilityPack;
using SimpleThingsProvider.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SimpleThingsProvider.Modules
{
    class x1337 : IModule
    {
        public string Name { get { return "x1337"; } set { } }
        private List<string> _underlying;
        public ListView listview { get; set; }
        public HtmlDocument Doc { get; set; }
        public bool needsSubSelector { get { return false; } }
        public x1337(ListView lv) 
        {
            listview = lv;
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
                if (Settings.Default.ProxyEnabled)
                {
                    Doc = web.Load(buildString(toSearch), Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                }
                else { Doc = web.Load(buildString(toSearch)); }
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound} for {mainWindow.WebsiteSource.SelectedItem.ToString()}", "Website (Search)"); return HttpStatusCode.NotFound; }
            code = web.StatusCode;
            return code;
        }
        public List<string> getResults(HtmlDocument document)
        {
            _underlying = new List<string>();
            List<Result> results = new();

            HtmlNodeCollection list = document.DocumentNode.SelectNodes("/html/body/main/div/div/div/div[2]/div[1]/table/tbody/tr");

            if (list == null)
            {
                Logger.Log("No results found!", "Websites (getResults - x1337)");
                return new List<string>();
            }
            Logger.Log($"Found {list.Count} results", "Websites (getResults - x1337)");
            foreach (HtmlNode node in list)
            {
                // Descend into further nodes to reach the a(s) with the href(depth 3)
                IEnumerable<HtmlNode> descendants = node.Descendants(3);
                // Needs to be splitted along \n
                var splittedTexts = node.InnerText.Split("\n");

                results.Add(new Result() { Title = splittedTexts[2], Seeds = splittedTexts[3], Leechs = splittedTexts[4], Time = splittedTexts[5], Size = splittedTexts[6] });

                foreach (HtmlNode descendant in descendants)
                {
                    try
                    {
                        if (descendant.Attributes["href"].Value.Contains("torrent"))
                        {
                            _underlying.Add("https://www.1337xx.to" + descendant.Attributes["href"].Value);
                        }
                    }
                    catch (NullReferenceException) { continue; }
                }
            }
            Logger.Log($"Found {_underlying.Count} entries", "Websites (getResults - x1337)");
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
            listview.Visibility = Visibility.Visible;
            return _underlying;
        }
        public string getLink(int index)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(_underlying[index]);
            HtmlNode node = doc.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[1]/li[1]/a");
            return node.Attributes["href"].Value;
        }
        public string getLink(string gameURL) { return null; }
        private string buildString(string input)
        {
            // Used by x1337
            string url = "https://1337xx.to/search/";
            var toSearch = url + input;
            toSearch = toSearch.Replace(" ", "%20");
            toSearch = toSearch + "/1/";
            return toSearch;
        }
    }
}
