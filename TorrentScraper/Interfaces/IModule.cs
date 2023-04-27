using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Controls;

namespace SimpleThingsProvider.Interfaces
{
    internal interface IModule
    {
        string Name { get; set; }
        public HtmlDocument Doc { get; set; }
        public List<string> getResults(HtmlDocument document, ListView ResultsList);
        public String getLink(int index);
        public HttpStatusCode search(string toSearch);
    }
}
