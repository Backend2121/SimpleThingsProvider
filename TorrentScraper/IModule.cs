using HtmlAgilityPack;
using SimpleThingsProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Windows.Controls;

public interface IModule
{
    string Name { get; set; }
    public HtmlDocument Doc { get; set; }
    public bool needsSubSelector { get; }
    public LinksWindow linksWindow { get; }
    //public MainWindow mainWindow { get; set; }
    public Tuple<List<Result>, List<string>> getResults(HtmlDocument document);
    public string getLink(int index);
    public string getLink(string gameURL);
    public HttpStatusCode search(string toSearch);
    public void buildListView(GridView view);
}