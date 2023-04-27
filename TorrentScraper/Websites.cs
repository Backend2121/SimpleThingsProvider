using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;
using System.Net;
using System.Diagnostics;

namespace SimpleThingsProvider
{
    public class Websites
    {
        List<string> underlying;
        string whoami;
        MainWindow mainWindow;
        public HtmlDocument doc { get; set; }
        public HttpStatusCode Search(string toSearch, string caller)
        {
            whoami = caller;
            HttpStatusCode code;
            HtmlWeb web = new();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            web.UserAgent = "SimpleThingsProvider";

            Logger.Log("Searching for: " + toSearch + " in: " + whoami, "Website (Search)");
            try
            {
                switch (whoami)
                {
                    case ("RPGOnly"):
                        if (Settings.Default.ProxyEnabled) doc = web.Load("https://rpgonly.com/list-of-all-switch-games-nsp-xci/", Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                        else doc = web.Load("https://rpgonly.com/list-of-all-switch-games-nsp-xci/");
                        break;

                    case ("NxBrew"):
                        if (Settings.Default.ProxyEnabled) doc = web.Load("https://nxbrew.com/gameindex.html", Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                        else doc = web.Load("https://nxbrew.com/gameindex.html");
                        break;

                    case ("Ziperto"):
                        if (Settings.Default.ProxyEnabled) doc = web.Load("https://www.ziperto.com/nintendo-switch-nsp-list/", Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                        else doc = web.Load("https://www.ziperto.com/nintendo-switch-nsp-list/");
                        break;

                    case ("HexRom"):
                        if (Settings.Default.ProxyEnabled) doc = web.Load("https://hexrom.com/roms/nintendo-3ds/?title=" + toSearch, Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                        else doc = web.Load("https://hexrom.com/roms/nintendo-3ds/?title=" + toSearch);
                        break;

                    case ("WoWRoms"):
                        if (Settings.Default.ProxyEnabled) doc = web.Load("https://wowroms.com/en/roms/list/" + mainWindow.WebsiteSubSelector.SelectedItem.ToString().Replace(" ", "%2B") + "?search=" + toSearch, Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                        else doc = web.Load("https://wowroms.com/en/roms/list/" + mainWindow.WebsiteSubSelector.SelectedItem.ToString().Replace(" ", "%2B") + "?search=" + toSearch);
                        break;

                    case ("FitGirl"):
                        if (Settings.Default.ProxyEnabled) doc = web.Load("https://fitgirl-repacks.site/?s=" + toSearch.Replace(" ", "+"), Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                        else doc = web.Load("https://fitgirl-repacks.site/?s=" + toSearch.Replace(" ", "+"));
                        break;

                    case ("VimmsLair"):
                        if (Settings.Default.ProxyEnabled) doc = web.Load("https://vimm.net/vault/?p=list&q=" + toSearch.Replace(" ", "+"), Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                        else doc = web.Load("https://vimm.net/vault/?p=list&q=" + toSearch.Replace(" ", "+"));
                        break;
                }
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound} for {mainWindow.WebsiteSource.SelectedItem.ToString()}", "Website (Search)"); return HttpStatusCode.NotFound; }
            
            code = web.StatusCode;
            return code;
        }
        public List<String> getResults(HtmlDocument document, string toSearch)
        {
            //Called by the main window, serves the purpose of switching based on the selection in the dropdown box
            Logger.Log("Switching for different websites", $"Websites (getResults - {whoami})");
            mainWindow = (MainWindow)Application.Current.MainWindow;
            switch (whoami)
            {
                case ("Ziperto"):
                    return getResults_Ziperto(document, mainWindow.ZipertoResultsList, toSearch);
                case ("HexRom"):
                    return getResults_HexRom(document, mainWindow.HexRomResultsList);
                case ("FitGirl"):
                    return getResults_FitGirl(document, mainWindow.FitGirlResultsList);
                case ("VimmsLair"):
                    return getResults_VimmsLair(document, mainWindow.VimmResultsList);
            }
            return new List<string>();
        }
        public List<String> getResults_Ziperto(HtmlDocument document, ListView ResultsList, string toSearch)
        {
            ResultsList.Visibility = Visibility.Visible;
            underlying = new List<string>();
            List<Result> results = new();
            HtmlNodeCollection games = document.DocumentNode.SelectNodes("/html/body/div[1]/div/div/div/div/div[1]/div[2]/article/div/div[1]/div[2]/div/div/ul/li/a");
            try
            {
                Logger.Log($"Found {games.Count} results", "Websites (getResults - Ziperto)");
                foreach (HtmlNode game in games)
                {
                    if (game.InnerText.ToLower().Contains(toSearch.ToLower()))
                    {
                        try
                        {
                            var title = game.InnerText.Replace("\t", "");
                            title = title.Replace("\n", "");
                            title = title.Replace("&amp;", "&");
                            title = title.Replace("&#8211;", "-");
                            results.Add(new Result() { Title = title });
                            underlying.Add(game.Attributes["href"].Value);
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

            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - Ziperto)");
            if (!Settings.Default.NSFWContent)
            {
                foreach (Result res in results)
                {
                    foreach (string s in BannedWords.nsfwWords)
                    {
                        if (res.Title.ToLower().Contains(s.ToLower()))
                        {
                            res.Title = "NSFW Content";
                            underlying[results.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_HexRom(HtmlDocument document, ListView ResultsList)
        {
            ResultsList.Visibility = Visibility.Visible;
            underlying = new List<string>();
            List<Result> results = new();
            HtmlNodeCollection alist = document.DocumentNode.SelectNodes("/html/body/div[2]/div[1]/div/div[1]/div/div/ul/li/a");
            try
            {
                Logger.Log($"Found {alist.Count} results", "Websites (getResults - HexRom)");
                foreach (HtmlNode game in alist)
                {
                    try
                    {
                        results.Add(new Result() { Title = game.Attributes["title"].Value});
                        underlying.Add(game.Attributes["href"].Value);
                    }
                    catch { continue; }
                }
            }
            catch(NullReferenceException)
            {
                Logger.Log("No results found!", "Websites (getResults - HexRom)");
                return new List<string>();
            }
            
            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - HexRoms)");
            if (!Settings.Default.NSFWContent)
            {
                foreach (Result res in results)
                {
                    foreach (string s in BannedWords.nsfwWords)
                    {
                        if (res.Title.ToLower().Contains(s.ToLower()))
                        {
                            res.Title = "NSFW Content";
                            underlying[results.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_FitGirl(HtmlDocument document, ListView ResultsList)
        {
            ResultsList.Visibility = Visibility.Visible;
            underlying = new List<string>();
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
                    string size = p.Substring(sizeStart, sizeEnd-sizeStart);
                    size = size.Replace("Original Size:", "");
                    

                    // repackSize section
                    var repacksizeStart = p.IndexOf("Repack Size:");
                    var repacksizeEnd = p.IndexOf("Download Mirrors");
                    string repacksize = p.Substring(repacksizeStart, repacksizeEnd - repacksizeStart);
                    repacksize = repacksize.Replace("Repack Size:", "");

                    //Link section
                    underlying.Add(game.LastChild.Attributes["href"].Value);

                    results.Add(new Result() { Title = title, OriginalSize = size, RepackSize = repacksize});
                }
            }
            catch(NullReferenceException)
            {
                Logger.Log("No results found!", "Websites (getResults - FitGirl)");
                return new List<string>();
            }

            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - FitGirl)");
            if (!Settings.Default.NSFWContent)
            {
                foreach (Result res in results)
                {
                    foreach (string s in BannedWords.nsfwWords)
                    {
                        if (res.Title.ToLower().Contains(s.ToLower()))
                        {
                            res.Title = "NSFW Content";
                            underlying[results.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_VimmsLair(HtmlDocument document, ListView ResultsList)
        {
            ResultsList.Visibility = Visibility.Visible;
            underlying = new List<string>();
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
                    underlying.Add("https://vimm.net" + tds[1].FirstChild.Attributes["href"].Value);
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
                            underlying[results.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            ResultsList.ItemsSource = results;
            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - VimmsLair)");
            return underlying;
        }
        public string getGamePage_Ziperto(string gameURL)
        {
            HtmlWeb web = new HtmlWeb();
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.LinksList.Visibility = Visibility.Visible;
            linksWindow.Show();
            doc = web.Load(gameURL);
            List<Result> websites = new List<Result>();
            Logger.Log($"Getting game page links", "Websites (getGamePage - Ziperto)");

            HtmlNodeCollection baseGame = doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div/div/div/div[1]/div[2]/article/div/div[2]/div[1]/p/span/strong/a");
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
            HtmlNodeCollection restGame = doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div/div/div/div[1]/div[2]/article/div/div[2]/div[1]/p/strong/a");
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
                            underlying[websites.IndexOf(res)] = string.Empty;
                        }
                        if (res.Infos.ToLower().Contains(s.ToLower()))
                        {
                            res.Infos = "NSFW Content";
                            underlying[websites.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            linksWindow.LinksList.ItemsSource = websites;
            return "";
        }
        public string getGamePage_HexRom(string gameURL)
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
                            underlying[websites.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            linksWindow.HexRomsLinksList.ItemsSource = websites;
            return "";
        }
        public string getGamePage_FitGirl(string gameURL)
        {
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.Show();
            linksWindow.LinksList.Visibility = Visibility.Visible;
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
                            underlying[websites.IndexOf(res)] = string.Empty;
                        }
                    }
                }
            }
            linksWindow.LinksList.ItemsSource = websites;
            return "";
        }
        public string getGamePage_VimmsLair(string gameURL)
        {
            return gameURL;
        }
        public string getMagnet(int index)
        {
            if (underlying[index].Equals(string.Empty)) { Logger.Log("NSFW Content selected, returning", $"Websites (getMagnet - {whoami}"); return ""; }
            Logger.Log("Switching for link to return", $"Websites (getMagnet - {whoami})");
            switch (whoami)
            {
                case ("Ziperto"):
                    return getGamePage_Ziperto(underlying[index]);
                case ("HexRom"):
                    return getGamePage_HexRom(underlying[index] + "/download/");
                case ("FitGirl"):
                    return getGamePage_FitGirl(underlying[index]);
                case ("VimmsLair"):
                    return getGamePage_VimmsLair(underlying[index]);
            }
            return "";
        }
    }
}