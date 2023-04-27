using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace SimpleThingsProvider.Interfaces
{
    internal interface IModule
    {
        string Name { get; set; }
        public HtmlDocument Doc { get; set; }
        public List<string> getResults(HtmlDocument document, ListView ResultsList);
        public List<string> getResults(HtmlDocument document, ListView ResultsList, string toSearch);
        public string getLink(int index);
        public string getLink(string gameURL);
        public HttpStatusCode search(string toSearch);
    }
}
