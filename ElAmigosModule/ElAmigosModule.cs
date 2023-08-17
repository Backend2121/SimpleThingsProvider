using HtmlAgilityPack;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SimpleThingsProvider
{
    public class ElAmigosModule : IModule
    {
        public string Name { get { return "ElAmigos"; } set { } }
        public HtmlDocument Doc { get; set; }
        private List<string> _underlying;
        public bool needsSubSelector { get { return false; } }
        public LinksWindow linksWindow { get { return new LinksWindow(); } }
        public ElAmigosModule() 
        {
            
        }

        public void buildListView(GridView grid)
        {
            GridViewColumn title = new GridViewColumn();
            title.Header = "Title";
            title.DisplayMemberBinding = new Binding("Title");
            GridViewColumn info = new GridViewColumn();
            info.Header = "Infos";
            info.DisplayMemberBinding = new Binding("Infos");
            grid.Columns.Add(title);
            grid.Columns.Add(info);
        }

        public string getLink(int index)
        {
            return getLink(_underlying[index]);
        }

        public string getLink(string gameURL)
        {
            LinksWindow linksWindow = new LinksWindow();
            linksWindow.Show();
            linksWindow.getLinksList().Visibility = Visibility.Visible;
            List<Result> websites = new List<Result>();
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(gameURL);
            HtmlNodeCollection downloads = doc.DocumentNode.SelectNodes("/html/body/div/div[6]/div/a");
            Result result = new Result();
            foreach (HtmlNode node in downloads)
            {
                result = new Result();
                result.Name = node.InnerText;
                result.Link = node.Attributes["href"].Value;
                result.Infos = "Base game";
                websites.Add(result);
            }

            string info = doc.DocumentNode.SelectSingleNode("/html/body/div/div[8]/p/text()").InnerText;
            downloads = doc.DocumentNode.SelectNodes("/html/body/div/div[8]/p/a");
            foreach (HtmlNode node in downloads)
            {
                result = new Result();
                result.Name = node.InnerText;
                result.Link = node.Attributes["href"].Value;
                result.Infos = info;
                websites.Add(result);
            }
            linksWindow.getLinksList().ItemsSource = websites;
            return "";
        }

        public Tuple<List<Result>, List<string>> getResults(HtmlDocument document)
        {
            _underlying = new List<string>();
            List<Result> results = new();

            HtmlNodeCollection res = document.DocumentNode.SelectNodes("/html/body/div/div[4]/div/div/div");
            foreach (HtmlNode node in res)
            {
                HtmlNode n = node.SelectSingleNode("h6/a");
                Result result = new Result();
                string title = n.InnerText;
                title = title.Replace("&#039;", "'");
                title = title.Replace("&#8217;", "’");
                title = title.Replace("&#8211;", "-");
                title = title.Replace("&#038;", "&");
                result.Title = title;
                
                result.Link = n.Attributes["href"].Value;
                n = node.SelectSingleNode("small");
                result.Infos = n.InnerText;
                if (!result.Infos.Contains("[Offer]"))
                {
                    results.Add(result);
                    _underlying.Add(result.Link);
                }
            }
            return Tuple.Create(results, _underlying);
        }

        public HttpStatusCode search(string toSearch)
        {
            HttpStatusCode code;
            HtmlWeb web = new();
            web.UserAgent = "SimpleThingsProvider";

            Logger.Log("Searching for: " + toSearch + " in: " + Name, "Website (Search)");
            try
            {
                if (Settings.Default.ProxyEnabled) Doc = web.Load("https://www.elamigos-games.com/?q=" + toSearch.Replace(" ", "+"), Settings.Default.ProxyIP, Int32.Parse(Settings.Default.ProxyPort), string.Empty, string.Empty);
                else Doc = web.Load("https://www.elamigos-games.com/?q=" + toSearch.Replace(" ", "+"));
            }
            catch
            {
                return HttpStatusCode.NotFound;
            }

            code = web.StatusCode;
            return code;
        }

        public async void checkUpdate()
        {
            string repoURL = "https://raw.githubusercontent.com/Backend2121/SimpleThingsProvider/Development/ElAmigosModule/Info.json";
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, repoURL);
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("User-Agent:", "SimpleThingsProvider"));
            HttpResponseMessage response = await client.SendAsync(requestMessage);
            string content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(content);
        }
    }
}