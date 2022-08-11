using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;

namespace SimpleThingsProvider
{
    public class Websites
    {
        List<string> underlying;
        string whoami;
        public class Result
        {
            public string Title { get; set; }
            public string Seeds { get; set; }
            public string Leechs { get; set; }
            public string Time { get; set; }
            public string Size { get; set; }

        }
        public class GameWebsite
        {
            public string Name { get; set; }
            public string Link { get; set; }
            public string Infos { get; set; }
        }
        public HtmlDocument doc { get; set; }
        public HttpStatusCode Search(string toSearch, string caller)
        {
            whoami = caller;
            HttpStatusCode code = HttpStatusCode.OK;
            HtmlWeb web = new HtmlWeb();
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            web.UserAgent = "SimpleThingsProvider";
            Logger.Log("Searching for: " + toSearch, "Website (Search)");
            try
            {
                switch (whoami)
                {
                    case ("x1337"):
                        doc = web.Load(buildString(toSearch));
                        break;

                    case ("ThePirateBay"):
                        doc = web.Load("https://tpb.party/search/" + toSearch);
                        break;

                    case ("RPGOnly"):
                        doc = web.Load("https://rpgonly.com/list-of-all-switch-games-nsp-xci/");
                        break;

                    case ("NxBrew"):
                        doc = web.Load("https://nxbrew.com/gameindex.html");
                        break;

                    case ("HexRom"):
                        doc = web.Load("https://hexrom.com/roms/nintendo-3ds/?title=" + toSearch);
                        break;

                    case ("WoWRoms"):
                        doc = web.Load("https://wowroms.com/en/roms/list/" + mainWindow.WebsiteSubSelector.SelectedItem.ToString().Replace(" ", "%2B") + "?search=" + toSearch);
                        break;

                    case ("FitGirl"):
                        doc = web.Load("https://fitgirl-repacks.site/?s=" + toSearch.Replace(" ", "+"));
                        break;
                }
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound} for {mainWindow.WebsiteSource.SelectedItem.ToString()}", "Website (Search)"); return HttpStatusCode.NotFound; }
            
            code = web.StatusCode;
            return code;
        }
        private string buildString(string input)
        {
            /// Used by x1337
            string url = "https://1337xx.to/search/";
            var toSearch = url + input;
            toSearch = toSearch.Replace(" ", "%20");
            toSearch = toSearch + "/1/";
            return toSearch;
        }
        public List<String> getResults(HtmlDocument document, ListView ResultsList, string toSearch)
        {
            //Called by the main window, serves the purpose of switching based on the selection in the dropdown box
            Logger.Log("Switching for different websites", $"Websites (getResults - {whoami})");
            switch (whoami)
            {
                case ("x1337"):
                    return getResults_x1337(document, ResultsList);
                case ("ThePirateBay"):
                    return getResults_ThePirateBay(document, ResultsList);
                case ("RPGOnly"):
                    return getResults_RPGOnly(document, ResultsList, toSearch);
                case ("NxBrew"):
                    return getResults_NxBrew(document, ResultsList, toSearch);
                case ("HexRom"):
                    return getResults_HexRom(document, ResultsList);
                case ("WoWRoms"):
                    return getResults_WoWRoms(document, ResultsList);
                case ("FitGirl"):
                    return getResults_FitGirl(document, ResultsList);
            }
            return new List<string>();
        }
        public List<String> getResults_x1337(HtmlDocument document, ListView ResultsList)
        {
            underlying = new List<string>();
            List<Result> results = new List<Result>();

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
                            underlying.Add("https://www.1337xx.to" + descendant.Attributes["href"].Value);
                        }
                    }
                    catch (NullReferenceException) { continue; }
                }
            }
            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - x1337)");
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_ThePirateBay(HtmlDocument document, ListView ResultsList)
        {
            underlying = new List<string>();
            List<Result> results = new List<Result>();

            HtmlNodeCollection trList = document.DocumentNode.SelectNodes("/html/body/div[2]/div[3]/table[1]/tr");
            if (trList == null)
            {
                Logger.Log("No results found!", "Websites (getResults - ThePirateBay)");
                return new List<string>();
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
                    if (descendant.HasClass("detName")) { title = descendant.InnerText.Replace('\t', '\0'); title = title.Replace('\n', '\0'); title = title.Replace('\r', '\0');
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
                            underlying.Add(descendant.Attributes["href"].Value);
                        }
                    }
                    catch (NullReferenceException) { continue; }
                }
                results.Add(new Result() { Title = title, Seeds = seeds, Leechs = leechs, Time = time, Size = size.Replace("Size ", "") });
            }
            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - ThePirateBay)");
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_RPGOnly(HtmlDocument document, ListView ResultsList, string toSearch)
        {
            underlying = new List<string>();
            List<Result> results = new List<Result>();

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
                                if (descendant.InnerText.ToLower().Contains(toSearch.ToLower()))
                                {
                                    underlying.Add(descendant.Attributes["href"].Value);
                                    results.Add(new Result() { Title = descendant.InnerText });
                                }
                            }
                        }
                        catch (NullReferenceException) { continue; }
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Logger.Log("No results found!", "Websites (getResults - RPGOnly)");
                return new List<string>();
            }
            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - RPGOnly)");
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_NxBrew(HtmlDocument document, ListView ResultsList, string toSearch)
        {
            underlying = new List<string>();
            List<Result> results = new List<Result>();
            HtmlNodeCollection letters = document.DocumentNode.SelectNodes("/html/body/div/div/div/div/div[3]/div/div[2]/article/div/div[1]/div[2]/div/div/ul/li/a");
            try
            {
                Logger.Log($"Found {letters.Count} results", "Websites (getResults - NxBrew)");
                foreach (HtmlNode letter in letters)
                {
                    if (letter.InnerText.ToLower().Contains(toSearch.ToLower()))
                    {
                        try
                        {
                            var title = letter.InnerText.Replace("\t", "");
                            title = title.Replace("\n", "");
                            results.Add(new Result() { Title = title });
                            underlying.Add(letter.Attributes["href"].Value);
                        }
                        catch (NullReferenceException) { continue; }
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Logger.Log("No results found!", "Websites (getResults - NxBrew)");
                return new List<string>();
            }
            
            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - NxBrew)");
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_HexRom(HtmlDocument document, ListView ResultsList)
        {
            underlying = new List<string>();
            List<Result> results = new List<Result>();
            HtmlNodeCollection alist = document.DocumentNode.SelectNodes("/html/body/div[2]/div[1]/div/div[1]/div/div/ul/li/a");
            try
            {
                Logger.Log($"Found {alist.Count} results", "Websites (getResults - HexRom)");
                foreach (HtmlNode game in alist)
                {
                    try
                    {
                        results.Add(new Result() { Title = game.Attributes["title"].Value });
                        underlying.Add(game.Attributes["href"].Value);
                    }
                    catch { continue; }
                }
            }
            catch(NullReferenceException e)
            {
                Logger.Log("No results found!", "Websites (getResults - HexRom)");
                return new List<string>();
            }
            
            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - HexRoms)");
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_WoWRoms(HtmlDocument document, ListView ResultsList)
        {
            underlying = new List<string>();
            List<Result> results = new List<Result>();

            HtmlNodeCollection games = document.DocumentNode.SelectNodes("/html/body/div[1]/div/div/section/div[2]/div[5]/ul/li/ul/li[2]/div/a[1]");
            try
            {
                Logger.Log($"Found {games.Count} results", "Websites (getResults - WoWRoms)");
                foreach (HtmlNode game in games)
                {
                    var title = game.InnerText;
                    title = title.Replace("\t", "");
                    title = title.Replace("\n", "");
                    title = title.Replace("\r", "");
                    title = title.Replace("  ", "");
                    results.Add(new Result { Title = title });
                    underlying.Add(game.Attributes["href"].Value);
                }
            }
            catch
            {
                Logger.Log("No results found!", "Websites (getResults - WoWRoms)");
                return new List<string>();
            }
            
            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - WoWRoms)");
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public List<String> getResults_FitGirl(HtmlDocument document, ListView ResultsList)
        {
            underlying = new List<string>();
            List<Result> results = new List<Result>();

            HtmlNodeCollection games = document.DocumentNode.SelectNodes("/html/body/div/div/section/div/article/header/h1/a");
            try
            {
                Logger.Log($"Found {games.Count} results", "Websites (getResults - FitGirl)");
                foreach (HtmlNode game in games)
                {
                    var title = game.InnerText;
                    title = title.Replace("&#8217;", "’");
                    title = title.Replace("&#8211;", "-");
                    title = title.Replace("&#038;", "&");
                    results.Add(new Result() { Title = title });
                    underlying.Add(game.Attributes["href"].Value);
                }
            }
            catch(NullReferenceException e)
            {
                Logger.Log("No results found!", "Websites (getResults - FitGirl)");
                return new List<string>();
            }

            Logger.Log($"Found {underlying.Count} entries", "Websites (getResults - FitGirl)");
            ResultsList.ItemsSource = results;
            return underlying;
        }
        public string getGamePage_RPGOnly(string gameURL)
        {
            HtmlWeb web = new HtmlWeb();
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.Show();
            doc = web.Load(gameURL);
            List<GameWebsite> websites = new List<GameWebsite>();
            HtmlNode entry = doc.DocumentNode.SelectSingleNode("/html/body/div/div[6]/div/div[1]/div/main/div[1]/article/div/figure[2]/table/tbody");
            Logger.Log($"Getting game page links", "Websites (getGamePage - RPGOnly)");
            var infos = "";
            var name = "";
            var link = "";
            foreach(HtmlNode descendant in entry.ChildNodes)
            {
                //We are selected the trs
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
                            websites.Add(new GameWebsite() { Infos = infos, Name = name, Link = link });
                        }
                    }
                    catch { link = ""; continue; }
                }
            }
            Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - RPGOnly)");
            linksWindow.LinksList.ItemsSource = websites;
            return "";
        }
        public string getGamePage_NxBrew(string gameURL)
        {
            HtmlWeb web = new HtmlWeb();
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.Show();
            doc = web.Load(gameURL);
            List<GameWebsite> websites = new List<GameWebsite>();
            Logger.Log($"Getting game page links", "Websites (getGamePage - NxBrew)");
            var title = "";
            var link = "";
            var infos = "";
            bool added = false;

            HtmlNodeCollection divs = doc.DocumentNode.SelectNodes("/html/body/div[1]/div/div/div/div[3]/div[1]/div/article/div[4]/div/div");
            foreach (HtmlNode div in divs)
            {
                if (div.HasClass("wp-block-column"))
                {
                    foreach (HtmlNode node in div.Descendants("p"))
                    {
                        if (node.HasClass("has-medium-font-size"))
                        {
                            added = false;
                            title = node.InnerText;
                            link = "";
                            infos = "";
                        }
                        else
                        {
                            try
                            {
                                title = "";
                                if (node.InnerText.Contains("Download"))
                                {
                                    added = false;
                                    infos = node.Descendants("strong").ToArray()[0].InnerText;
                                    link = node.Descendants("a").ToArray()[0].Attributes["href"].Value;
                                }
                                else
                                {
                                    foreach (HtmlNode node2 in node.Descendants("a"))
                                    {
                                        infos = node2.InnerText;
                                        link = node2.Attributes["href"].Value;
                                        websites.Add(new GameWebsite() { Name = infos, Link = link, Infos = title });
                                        added = true;
                                    }
                                }
                            }
                            catch (IndexOutOfRangeException) { continue; }
                        }
                        if (!added) { websites.Add(new GameWebsite() { Name = infos, Link = link, Infos = title }); }
                        
                    }
                }
            }
            Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - NxBrew)");
            linksWindow.LinksList.ItemsSource = websites;
            return "";
        }
        public string getGamePage_HexRom(string gameURL)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(gameURL);
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.Show();
            List<GameWebsite> websites = new List<GameWebsite>();
            var title = "";
            var link = "";
            var infos = "";
            bool added = false;
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("/html/body/div[2]/div[1]/div/div/div/div[2]/div/table/tbody/tr/td/a");
            Logger.Log($"Getting game page links", "Websites (getGamePage - HexRom)");
            foreach (HtmlNode downloadlink in links)
            {
                title = downloadlink.InnerText;
                link = downloadlink.Attributes["href"].Value;
                if (title != " ") { websites.Add(new GameWebsite() { Link = link, Infos = title }); }   
            }
            Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - HexRom)");
            linksWindow.LinksList.ItemsSource = websites;
            return "";
        }
        public string getGamePage_WoWRoms(string gameURL)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(gameURL);
            Logger.Log($"Getting the inner link from WoWRoms", "Websites (getGamePage - WoWRoms)");
            return "https://wowroms.com" + doc.DocumentNode.SelectSingleNode("/html/body/div/div/div/section/div[2]/div[2]/div[1]/div[2]/div/div[2]/a").Attributes["href"].Value;
        }
        public string getGamePage_FitGirl(string gameURL)
        {
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.Show();
            List<GameWebsite> websites = new List<GameWebsite>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(gameURL);
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("/html/body/div/div/div[1]/div/article/div/ul[1]/li/a");
            Logger.Log($"Getting game page links", "Websites (getGamePage - FitGirl)");
            foreach (HtmlNode downloadlink in links)
            {
                websites.Add(new GameWebsite() { Link = downloadlink.Attributes["href"].Value, Infos = downloadlink.InnerText });
            }
            Logger.Log($"Found {websites.Count} game page links", "Websites (getGamePage - FitGirl)");
            linksWindow.LinksList.ItemsSource = websites;
            return "";
        }
        public string getMagnet(int index)
        {
            Logger.Log("Switching for link to return", $"Websites (getMagnet - {whoami})");
            switch (whoami)
            {
                case ("x1337"):
                    HtmlWeb web = new HtmlWeb();
                    HtmlDocument doc = web.Load(underlying[index]);
                    HtmlNode node = doc.DocumentNode.SelectSingleNode("/html/body/main/div/div/div/div[2]/div[1]/ul[1]/li[1]/a");
                    return node.Attributes["href"].Value;
                case ("ThePirateBay"):
                    return underlying[index];
                case ("RPGOnly"):
                    return getGamePage_RPGOnly(underlying[index]);
                case ("NxBrew"):
                    return getGamePage_NxBrew(underlying[index]);
                case ("HexRom"):
                    return getGamePage_HexRom(underlying[index] + "/download/");
                case ("WoWRoms"):
                    return getGamePage_WoWRoms("https://wowroms.com" + underlying[index]);
                case ("FitGirl"):
                    return getGamePage_FitGirl(underlying[index]);
            }
            return "";
        }
    }
}
