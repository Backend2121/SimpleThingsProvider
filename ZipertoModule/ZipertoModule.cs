using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleThingsProvider
{
    internal class ZipertoModule : IModule
    {
        public string Name { get { return "Ziperto"; } set { } }
        public string ModuleVersion { get { return "1.0.0"; } set { } }
        public HtmlDocument Doc { get; set; }
        private List<string> _underlying;
        private string _toSearch;
        public bool needsSubSelector { get { return false; } }
        public LinksWindow linksWindow { get { return new LinksWindow(); } }

        public ZipertoModule()
        {

        }
        public void buildListView(GridView grid)
        {
            GridViewColumn title = new GridViewColumn();
            title.Header = "Mahnz";
            title.DisplayMemberBinding = new Binding("Title");
            grid.Columns.Add(title);
        }
        public string getLink(int index)
        {
            return getLink(_underlying[index]);
        }

        public string getLink(string gameURL)
        {
            HtmlWeb web = new HtmlWeb();
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.getLinksList().Visibility = Visibility.Visible;
            linksWindow.Show();
            Doc = web.Load(gameURL);
            List<Result> websites = new List<Result>();
            Logger.Log($"Getting game page links", "Websites (getGamePage - Ziperto)");

            HtmlNodeCollection baseGame = Doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div/div/div/div[1]/div[2]/article/div/div[2]/div[1]/p/span/strong/a");
            try
            {
                foreach (HtmlNode node in baseGame)
                {
                    string infos = node.ParentNode.ParentNode.ParentNode.InnerText;
                    infos = infos.Replace("&#8212;", "");
                    infos = infos.Replace("&#8211;", "");
                    websites.Add(new Result() { Infos = infos, Name = node.InnerText, Link = node.Attributes["href"].Value });
                }
            }
            catch { }
            Debug.WriteLine("baseGame: " + websites.Count);
            HtmlNodeCollection restGame = Doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div/div/div/div[1]/div[2]/article/div/div[2]/div[1]/p/strong/a");
            int counter = -1;
            Debug.WriteLine("restGame: " + restGame.Count);
            try
            {
                foreach (var p in restGame)
                {
                    var info = p.ParentNode.ParentNode.InnerText;
                    info = info.Replace("&#8212;", "");
                    info = info.Replace("Part1", "");
                    info = info.Replace("Part2", "");
                    info = info.Replace("Part3", "");
                    info = info.Replace("&#8211;", ":");
                    int startList = info.LastIndexOf(':');
                    string[] strings = info.Substring(startList).Split("-&gt;");
                    if (p.InnerText.Contains("Part1")) { counter++; }
                    try
                    {
                        websites.Add(new Result() { Infos = info, Name = strings[counter] + "- " + p.InnerText, Link = p.Attributes["href"].Value });
                    }
                    catch (IndexOutOfRangeException)
                    {
                        try
                        {
                            if (p.InnerText.Contains("Part"))
                            {
                                websites.Add(new Result() { Infos = p.ParentNode.ParentNode.FirstChild.InnerText, Name = "1Fichier" + " - " + p.InnerText, Link = p.Attributes["href"].Value });
                            }
                            else
                            {
                                websites.Add(new Result() { Infos = p.ParentNode.ParentNode.FirstChild.InnerText, Name = p.InnerText, Link = p.Attributes["href"].Value });
                            }
                        }
                        catch { continue; }
                    }
                    if (counter == strings.Length - 2) { counter = -1; }
                }
            }
            catch { }
            Debug.WriteLine("Ending: " + websites.Count);
            Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - Ziperto)");
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

        public Tuple<List<Result>, List<string>> getResults(HtmlDocument document)
        {
            _underlying = new List<string>();
            List<Result> results = new();
            HtmlNodeCollection games = document.DocumentNode.SelectNodes("/html/body/div[1]/div/div/div/div/div[1]/div[2]/article/div/div[1]/div[2]/div/div/ul/li/a");
            try
            {
                Logger.Log($"Found {games.Count} results", "Websites (getResults - Ziperto)");
                foreach (HtmlNode game in games)
                {
                    if (game.InnerText.ToLower().Contains(_toSearch.ToLower()))
                    {
                        try
                        {
                            var title = game.InnerText.Replace("\t", "");
                            title = title.Replace("\n", "");
                            title = title.Replace("&amp;", "&");
                            title = title.Replace("&#8211;", "-");
                            results.Add(new Result() { Title = title });
                            _underlying.Add(game.Attributes["href"].Value);
                        }
                        catch (NullReferenceException) { continue; }
                    }
                }
            }
            catch (NullReferenceException)
            {
                Logger.Log("No results found!", "Websites (getResults - Ziperto)");
                return Tuple.Create(new List<Result>(), new List<string>());
            }

            Logger.Log($"Found {_underlying.Count} entries", "Websites (getResults - Ziperto)");
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
            _toSearch = toSearch;
            HttpStatusCode code;
            HtmlWeb web = new();
            web.UserAgent = "SimpleThingsProvider";

            Logger.Log("Searching for: " + toSearch + " in: " + Name, "Website (Search)");

            try
            {
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://www.ziperto.com/nintendo-switch-nsp-list/", Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://www.ziperto.com/nintendo-switch-nsp-list/");
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound}", "Website (Search)"); return HttpStatusCode.NotFound; }

            code = web.StatusCode;
            return code;
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
