using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Net;
using System.Windows.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http;

namespace SimpleThingsProvider
{
    class TPBModule : IModule
    {
        public string Name { get { return "ThePirateBay"; } set { } }
        public string ModuleVersion { get { return "1.0.0"; } set { } }
        private List<string> _underlying;
        public HtmlDocument Doc { get; set; }
        public bool needsSubSelector { get { return false; } }
        public LinksWindow linksWindow { get { return new LinksWindow(); } }
        public TPBModule()
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
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://tpb.party/search/" + toSearch, Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://tpb.party/search/" + toSearch);
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound}", "Website (Search)"); return HttpStatusCode.NotFound; }

            return web.StatusCode;
        }
        public Tuple<List<Result>, List<string>> getResults(HtmlDocument document)
        {
            _underlying = new List<string>();
            List<Result> results = new();

            HtmlNodeCollection trList = document.DocumentNode.SelectNodes("/html/body/div[2]/div[3]/table[1]/tr");
            if (trList == null)
            {
                Logger.Log("No results found!", "Websites (getResults - ThePirateBay)");
                return Tuple.Create(new List<Result>(), new List<string>());
            }
            Logger.Log($"Found {trList.Count} results", "Websites (getResults - ThePirateBay)");
            // Foreach tr find all the 4 td(s)
            foreach (HtmlNode tr in trList)
            {

                IEnumerable<HtmlNode> descendants = tr.Descendants(1);
                // First descendant is useless
                var title = "";
                var seeds = "";
                var leechs = "";
                var time = "";
                var size = "";
                bool secondTime = false;

                foreach (HtmlNode descendant in descendants)
                {
                    if (descendant.HasClass("detName"))
                    {
                        title = descendant.InnerText.Replace('\t', '\0'); title = title.Replace('\n', '\0'); title = title.Replace('\r', '\0');
                    }
                    if (descendant.HasClass("detDesc"))
                    {
                        var infos = descendant.InnerText.Replace("&nbsp;", "");

                        var splittedInfos = infos.Split(",");
                        for (int i = 0; i < splittedInfos.Length; i++)
                        {
                            if (splittedInfos[i].Contains("Uploaded")) { time = splittedInfos[i].Replace("Uploaded", ""); }
                            if (splittedInfos[i].Contains("Size")) { size = splittedInfos[i].Replace("Size ", ""); }
                        }
                    }
                    if (descendant.OuterHtml.Contains("align="))
                    {
                        if (secondTime) { leechs = descendant.InnerText; }
                        else { seeds = descendant.InnerText; secondTime = true; }
                    }
                    try
                    {
                        if (descendant.Attributes["href"].Value.Contains("magnet"))
                        {
                            _underlying.Add(descendant.Attributes["href"].Value);
                        }
                    }
                    catch (NullReferenceException) { continue; }
                }
                results.Add(new Result() { Title = title, Seeds = seeds, Leechs = leechs, Time = time, Size = size.Replace("Size ", "") });
            }
            Logger.Log($"Found {_underlying.Count} entries", "Websites (getResults - ThePirateBay)");
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
        public String getLink(int index)
        {
            return _underlying[index];
        }
        public string getLink(string gameURL) { return null; }
        public async void checkUpdate()
        {
            string repoURL = "https://raw.githubusercontent.com/Backend2121/SimpleThingsProvider/master/TPBModule/Info.json";
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
