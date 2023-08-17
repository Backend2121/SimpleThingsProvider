using HtmlAgilityPack;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace SimpleThingsProvider
{
    class x1337Module : IModule
    {
        public string Name { get { return "x1337"; } set { } }
        public string ModuleVersion { get { return "1.0.0"; } set { } }
        private List<string> _underlying;
        public HtmlDocument Doc { get; set; }
        public bool needsSubSelector { get { return false; } }
        public LinksWindow linksWindow { get { return new LinksWindow(); } }

        public x1337Module()
        {

        }
        public void buildListView(GridView grid)
        {
                GridViewColumn title = new GridViewColumn
                {
                    Header = "Title",
                    DisplayMemberBinding = new Binding("Title"),
                    Width = Double.NaN,
                };

                GridViewColumn seeds = new GridViewColumn
                {
                    Header = "Seeds",
                    DisplayMemberBinding = new Binding("Seeds"),
                    Width = Double.NaN,
                };

                GridViewColumn leechs = new GridViewColumn
                {
                    Header = "Leechs",
                    DisplayMemberBinding = new Binding("Leechs"),
                    Width = Double.NaN,
                };

                GridViewColumn time = new GridViewColumn
                {
                    Header = "Time",
                    DisplayMemberBinding = new Binding("Time"),
                    Width = Double.NaN,
                };

                GridViewColumn size = new GridViewColumn
                {
                    Header = "Size",
                    DisplayMemberBinding = new Binding("Size"),
                    Width = Double.NaN,
                };
                grid.Columns.Clear();
                grid.Columns.Add(title);
                grid.Columns.Add(seeds);
                grid.Columns.Add(leechs);
                grid.Columns.Add(time);
                grid.Columns.Add(size);
        }

        public HttpStatusCode search(string toSearch)
        {
            HttpStatusCode code;
            HtmlWeb web = new();
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
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound} ", "Website (Search)"); return HttpStatusCode.NotFound; }
            code = web.StatusCode;
            Debug.Write(code);
            return code;
        }
        public Tuple<List<Result>, List<string>> getResults(HtmlDocument document)
        {
            _underlying = new List<string>();
            List<Result> results = new();

            HtmlNodeCollection list = document.DocumentNode.SelectNodes("/html/body/main/div/div/div/div[2]/div[1]/table/tbody/tr");

            if (list == null)
            {
                Logger.Log("No results found!", "Websites (getResults - x1337)");
                return Tuple.Create(new List<Result>(), new List<string>());
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
            return Tuple.Create(results, _underlying);
        }
        public string getLink(int index)
        {
            HtmlWeb web = new HtmlWeb();
            if (_underlying[index] != string.Empty)
            {
                HtmlDocument doc = web.Load(_underlying[index]);
                HtmlNode node = doc.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[1]/li[1]/a");
                Debug.WriteLine(_underlying[index]);
                Debug.WriteLine(node.Attributes["href"].Value);
                if (node.Attributes["href"].Value.Contains("magnet:?"))
                {
                    return node.Attributes["href"].Value;
                }
                else
                {
                    return _underlying[index];
                }
            }
            
            return string.Empty;
        }
        public string getLink(string gameURL) { return null; }
        private string buildString(string input)
        {
            string url = "https://1337xx.to/search/";
            var toSearch = url + input;
            toSearch = toSearch.Replace(" ", "%20");
            toSearch = toSearch + "/1/";
            return toSearch;
        }
        public async void checkUpdate()
        {
            string repoURL = "https://raw.githubusercontent.com/Backend2121/SimpleThingsProvider/Development/ElAmigosModule/Info.json";
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, repoURL);
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("User-Agent", "SimpleThingsProvider"));
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string content = await response.Content.ReadAsStringAsync();
            int start = content.IndexOf(": \"");
            Debug.WriteLine("Start: " + start);
            int end = content.IndexOf('"', start + 3);
            Debug.WriteLine("End: " + end);
            string version = content.Substring(start + 3, end - start - 3);
            Debug.WriteLine(version);
            Debug.WriteLine(ModuleVersion);
            if (!version.Equals(ModuleVersion))
            {
                AlertClass.Alert("An update for " + Name + " is available, open the GitHub page?", Name, MessageBoxButton.YesNo, MessageBoxImage.Information);
            }
        }
    }
}