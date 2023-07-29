using MahApps.Metro.Controls;
using SimpleThingsProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DownloaderExtension
{
    public class WebClientIdentified
    {
        public WebClient client { get; }
        public string name { get; }
        public bool isCancelled { get; set; }
        public WebClientIdentified(WebClient c, string n) 
        {
            client = c;
            name = n;
            isCancelled = false;
        }
    }
    public class DownloaderTemplate
    {
        public DockPanel e;
        public DownloaderTemplate(int i, string t, string u) 
        {
            e = new DockPanel();
            // Grid definitions
            Grid grid = new Grid();
            ColumnDefinition columnDefinition1 = new ColumnDefinition();
            ColumnDefinition columnDefinition2 = new ColumnDefinition();
            ColumnDefinition columnDefinition3 = new ColumnDefinition();
            ColumnDefinition columnDefinition4 = new ColumnDefinition();
            ColumnDefinition columnDefinition5 = new ColumnDefinition();
            columnDefinition1.Width = new GridLength(75, GridUnitType.Star);
            columnDefinition2.Width = new GridLength(100, GridUnitType.Star);
            columnDefinition3.Width = new GridLength(70, GridUnitType.Star);
            columnDefinition4.Width = new GridLength(400, GridUnitType.Star);
            columnDefinition5.Width = new GridLength(100, GridUnitType.Star);
            grid.ColumnDefinitions.Add(columnDefinition1);
            grid.ColumnDefinitions.Add(columnDefinition2);
            grid.ColumnDefinitions.Add(columnDefinition3);
            grid.ColumnDefinitions.Add(columnDefinition4);
            grid.ColumnDefinitions.Add(columnDefinition5);
            // The 3 labels
            Label titleLabel = new Label();
            Label ETALabel = new Label();
            Label percentageLabel = new Label();
            Label urlLabel = new Label();
            titleLabel.Content = t;
            titleLabel.Name = "titleLabel" + i.ToString();
            urlLabel.Content = u;
            urlLabel.Name = "urlLabel" + i.ToString();
            urlLabel.Visibility = Visibility.Collapsed;
            ETALabel.Content = "NULL";
            percentageLabel.Content = "%";
            // Progress bar
            ProgressBar progressBar = new ProgressBar();
            // Stop button
            Button stopButton = new Button();
            stopButton.Content = "S";
            Grid.SetColumn(titleLabel, 0);
            Grid.SetColumn(urlLabel, 0);
            Grid.SetColumn(ETALabel, 1);
            Grid.SetColumn(percentageLabel, 2);
            Grid.SetColumn(progressBar, 3);
            Grid.SetColumn(stopButton, 4);
            // Setting the margins
            titleLabel.Margin = new Thickness(0, 0, 10, 0);
            ETALabel.Margin = new Thickness(10, 0, 10, 0);
            percentageLabel.Margin = new Thickness(10, 0, 10, 0);
            progressBar.Margin = new Thickness(10, 5, 10, 5);
            stopButton.Margin = new Thickness(10, 0, 10, 0);
            grid.Margin = new Thickness(0, 0, 0, 10);
            grid.ShowGridLines = true;
            // Appending everything to the grid
            grid.Children.Add(titleLabel);
            grid.Children.Add(urlLabel);
            grid.Children.Add(ETALabel);
            grid.Children.Add(percentageLabel);
            grid.Children.Add(progressBar);
            grid.Children.Add(stopButton);
            e.Children.Add(grid);
        }
    }
    public partial class DownloaderWindow : Window
    {
        private int downloadNumber = 0;
        private Regex extensionExpression = new("(\\.)+(.{2,4})(?!.*\\1)");
        private ProgressBar progress;
        private Label percentage;
        public List<WebClientIdentified> webClients = new List<WebClientIdentified>();
        public DownloaderWindow()
        {
            InitializeComponent();
            /*int childCounter = 0;
            string json = "{\"Downloads\":{";
            // Read from file how many downloads needs to be instantiated
            downloadNumber = READ_DOWNLOADS;

            // Serializer
            // Serialize only if download number is grater than 0
            if (sp.Children.Count > 0 )
            {
                foreach (DockPanel dock in sp.Children)
                {
                    foreach (Grid grid in dock.Children)
                    {
                        UIElementCollection elements = grid.Children;
                        Label titlelabel = (Label)elements[0];
                        Label urllabel = (Label)elements[1];
                        // Serialize child element
                        json += "\"" + childCounter.ToString() + "\":{\"url\":\"" + urllabel.Content + "\",\"name\":\"" + titlelabel.Content + "\"},";
                    }
                    childCounter++;
                }
                json = json.Remove(json.Length - 1);
                json += "}}";
                File.WriteAllText(".\\downloads.json", json);
            }*/
        }
        public void addDownload(string title, string url)
        {
            downloadNumber++;
            sp.Children.Add(new DownloaderTemplate(downloadNumber, title, url).e);
            startDownload();
        }
        public void startDownload()
        {
            string url = string.Empty;
            string name = string.Empty;
            // Read from existing downloads informations needed to start them
            foreach (DockPanel dock in sp.Children)
            {
                foreach (Grid grid in dock.Children)
                {
                    UIElementCollection col = grid.Children;
                    name = ((Label)col[0]).Content.ToString();
                    url = ((Label)col[1]).Content.ToString();
                    Button stopBtn = (Button)col[5];
                    stopBtn.Name = "DownloadButton" + downloadNumber.ToString();
                    stopBtn.Click += StopButton_Click;
                    progress = ((ProgressBar)col[4]);
                    percentage = ((Label)col[3]);
                    Match match = extensionExpression.Match(url);
                    if (match.Success)
                    {
                        WebClient client = new WebClient();
                        WebClientIdentified webClientIdentified = new WebClientIdentified(client, downloadNumber.ToString());
                        //client.Headers WE NEED TO SET THEM
                        client.DownloadProgressChanged += client_DownloadProgressChanged;
                        client.DownloadFileCompleted += client_DownloadFileCompleted;
                        webClients.Add(webClientIdentified);
                        object[] payload = new object[] { progress, percentage, webClientIdentified };
                        client.DownloadFileAsync(
                            // Param1 = Link of file
                            new Uri(url),
                            // Param2 = Path to save, need to have this in Settings
                            "C:\\Users\\alexi\\Desktop\\" + name + match.Value,
                            payload
                        );
                    }
                    else
                    {
                        //Regex match not found, notify user
                    }
                }
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Picks the last progress and percentage label in the grid, how to fix this?
            object[] payload = (object[])e.UserState;
            ProgressBar pb = (ProgressBar)payload[0];
            Label p = (Label)payload[1];
            WebClientIdentified wc = (WebClientIdentified)payload[2];

            if (wc.isCancelled)
            {
                wc.client.DownloadProgressChanged -= client_DownloadProgressChanged;
                wc.client.DownloadFileCompleted -= client_DownloadFileCompleted;
                wc.client.Dispose();
                pb.Value = 0;
                p.Content = "Cancelled";
                return;
            }
            pb.Value = e.ProgressPercentage;
            p.Content = e.ProgressPercentage + "%";
        }
        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // Remove partial file here somehow
                return;
            }
            if (e.Error != null)
            {
                throw e.Error;
            }
            // Picks the last progress and percentage label in the grid, how to fix this?
            object[] payload = (object[])e.UserState;
            ProgressBar pb = (ProgressBar)payload[0];
            Label p = (Label)payload[1];
            pb.Value = 100;
            p.Content = "100%";
        }
        public void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Button me = (Button)sender;            
            foreach(WebClientIdentified webClientIdentified in webClients)
            {
                if (me.Name.Contains(webClientIdentified.name))
                {
                    Debug.WriteLine(webClientIdentified.client.BaseAddress);
                    webClientIdentified.isCancelled = true;
                }
            }
        }
    }
}