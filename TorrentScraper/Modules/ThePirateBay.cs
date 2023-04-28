using HtmlAgilityPack;
using SimpleThingsProvider.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Net;

namespace SimpleThingsProvider.Modules
{
    class ThePirateBay : IModule
    {
        public string Name { get { return "ThePirateBay"; } set { } }
        private List<string> _underlying;
        public HtmlDocument Doc { get; set; }
        public ListView _listview { get; set; }
        public ThePirateBay(ListView lv)
        {
            _listview = lv;
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
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://tpb.party/search/" + toSearch, Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://tpb.party/search/" + toSearch);
            }
            catch { Logger.Log($"Error code {HttpStatusCode.NotFound} for {mainWindow.WebsiteSource.SelectedItem.ToString()}", "Website (Search)"); return HttpStatusCode.NotFound; }

            return web.StatusCode;
        }
        public List<String> getResults(HtmlDocument document)
        {
            _underlying = new List<string>();
            List<Result> results = new();

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
            _listview.ItemsSource = results;
            _listview.Visibility = Visibility.Visible;
            return _underlying;
        }
        public String getLink(int index)
        {
            return _underlying[index];
        }
        public string getLink(string gameURL) { return null; }

    }
}
