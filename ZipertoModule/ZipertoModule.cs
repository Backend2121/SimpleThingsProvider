using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleThingsProvider
{
    internal class ZipertoModule : IModule
    {
        public string Name { get { return "Ziperto"; } set { } }
        public HtmlDocument Doc { get; set; }
        private List<string> _underlying;
        private string _toSearch;
        public MainWindow mainWindow { get { return (MainWindow)Application.Current.MainWindow; } set { } }
        public bool needsSubSelector { get { return false; } }
        public LinksWindow linksWindow { get { return new LinksWindow(); } }

        public ZipertoModule()
        {

        }
        public void buildListView()
        {
            GridView grid = new GridView();
            GridViewColumn title = new GridViewColumn();
            title.Header = "Mahnz";
            title.DisplayMemberBinding = new Binding("Title");
            grid.Columns.Add(title);
            mainWindow.getResultsList().View = grid;
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

        public List<string> getResults(HtmlDocument document)
        {
            mainWindow.getResultsList().Visibility = Visibility.Visible;
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
                return new List<string>();
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
            mainWindow.getResultsList().ItemsSource = results;
            buildListView();
            return _underlying;
        }

        public HttpStatusCode search(string toSearch)
        {
            _toSearch = toSearch;
            HttpStatusCode code;
            HtmlWeb web = new();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            web.UserAgent = "SimpleThingsProvider";

            Logger.Log("Searching for: " + toSearch + " in: " + Name, "Website (Search)");

            try
            {
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://www.ziperto.com/nintendo-switch-nsp-list/", Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://www.ziperto.com/nintendo-switch-nsp-list/");
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound} for {mainWindow.getWebsiteSource().SelectedItem.ToString()}", "Website (Search)"); return HttpStatusCode.NotFound; }

            code = web.StatusCode;
            return code;
        }
    }
}
