using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleThingsProvider
{
    class VimmslairModule : IModule
    {
        public string Name { get { return "VimmsLair"; } set { } }
        public HtmlDocument Doc { get; set; }
        private List<string> _underlying;
        public MainWindow mainWindow { get { return (MainWindow)Application.Current.MainWindow; } }
        public bool needsSubSelector { get { return false; } }
        public LinksWindow linksWindow { get { return new LinksWindow(); } }

        public VimmslairModule()
        {
            
        }
        public void buildListView()
        {
            GridView grid = new GridView();
            GridViewColumn title = new GridViewColumn();
            title.Header = "Title";
            title.DisplayMemberBinding = new Binding("Title");

            GridViewColumn system = new GridViewColumn();
            system.Header = "System";
            system.DisplayMemberBinding = new Binding("System");

            GridViewColumn region = new GridViewColumn();
            region.Header = "Region";
            region.DisplayMemberBinding = new Binding("Region");

            GridViewColumn version = new GridViewColumn();
            version.Header = "Version";
            version.DisplayMemberBinding = new Binding("Version");

            GridViewColumn languages = new GridViewColumn();
            languages.Header = "Languages";
            languages.DisplayMemberBinding = new Binding("Languages");

            grid.Columns.Add(title);
            grid.Columns.Add(system);
            grid.Columns.Add(region);
            grid.Columns.Add(version);
            grid.Columns.Add(languages);
            mainWindow.getResultsList().View = grid;
        }
        public string getLink(int index)
        {
            return getLink(_underlying[index]);
        }
        public string getLink(string gameURL)
        {
            return gameURL;
        }
        public List<string> getResults(HtmlDocument document)
        {
            mainWindow.getResultsList().Visibility = Visibility.Visible;
            _underlying = new List<string>();
            List<Result> results = new();
            HtmlNodeCollection games = document.DocumentNode.SelectNodes("/html/body/div[4]/div[2]/div/div[3]/table/tr");
            Logger.Log($"Found {games.Count} results", "Websites (getResults - VimmsLair)");
            foreach (HtmlNode game in games)
            {
                HtmlNodeCollection tds = game.SelectNodes("td");
                try
                {
                    var title = tds[1].InnerText;
                    title = title.Replace("&nbsp;", " ");
                    title = title.Replace("&#x26a0;", "");
                    title = title.Replace("&#039;", "'");

                    results.Add(new Result() { System = tds[0].InnerText, Title = title, Region = tds[2].FirstChild.Attributes["title"].Value, Version = tds[3].InnerText, Languages = tds[4].InnerText });
                    _underlying.Add("https://vimm.net" + tds[1].FirstChild.Attributes["href"].Value);
                }
                catch (NullReferenceException) { Logger.Log("No results found!", "Websites (getResults - VimmsLair)"); return new List<string>(); }
            }
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
            Logger.Log($"Found {_underlying.Count} entries", "Websites (getResults - VimmsLair)");
            buildListView();
            return _underlying;
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
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://vimm.net/vault/?p=list&q=" + toSearch.Replace(" ", "+"), Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://vimm.net/vault/?p=list&q=" + toSearch.Replace(" ", "+"));
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound} for {mainWindow.getWebsiteSource().SelectedItem.ToString()}", "Website (Search)"); return HttpStatusCode.NotFound; }

            code = web.StatusCode;
            return code;
        }
    }
}
