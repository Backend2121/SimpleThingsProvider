using HtmlAgilityPack;
using SimpleThingsProvider;
using System.Collections.Generic;
using System.Net;
using System.Windows.Controls;

public interface IModule
{
    string Name { get; set; }
    public HtmlDocument Doc { get; set; }
    public ListView listview { get;  set; }
    public bool needsSubSelector { get; }
    public List<string> getResults(HtmlDocument document);
    public string getLink(int index);
    public string getLink(string gameURL);
    public HttpStatusCode search(string toSearch);
}