using MahApps.Metro.Controls;
using SimpleThingsProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
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
    public class IsPaused
    {
        public bool pause { get; set; }
        public IsPaused(bool p)
        {
            pause = p;
        }
    }
    public class HttpClientPair
    {
        public CancellationTokenSource cancellation { get; set; }
        public string stopName { get; }
        public string pauseName { get; }
        public IsPaused isPaused { get; set; }
        public HttpClientPair(CancellationTokenSource c, string sn, string pn, IsPaused p) 
        {
            cancellation = c;
            stopName = sn;
            pauseName = pn;
            isPaused = p;
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
            ColumnDefinition columnDefinition6 = new ColumnDefinition();
            columnDefinition1.Width = new GridLength(75, GridUnitType.Star);
            columnDefinition2.Width = new GridLength(100, GridUnitType.Star);
            columnDefinition3.Width = new GridLength(70, GridUnitType.Star);
            columnDefinition4.Width = new GridLength(400, GridUnitType.Star);
            columnDefinition5.Width = new GridLength(67, GridUnitType.Star);
            columnDefinition6.Width = new GridLength(67, GridUnitType.Star);
            grid.ColumnDefinitions.Add(columnDefinition1);
            grid.ColumnDefinitions.Add(columnDefinition2);
            grid.ColumnDefinitions.Add(columnDefinition3);
            grid.ColumnDefinitions.Add(columnDefinition4);
            grid.ColumnDefinitions.Add(columnDefinition5);
            grid.ColumnDefinitions.Add(columnDefinition6);
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
            // Pause/Resume and Stop buttons
            Button stopButton = new Button();
            Button pauseButton = new Button();
            stopButton.Content = "S";
            pauseButton.Content = "P";
            Grid.SetColumn(titleLabel, 0);
            Grid.SetColumn(urlLabel, 0);
            Grid.SetColumn(ETALabel, 1);
            Grid.SetColumn(percentageLabel, 2);
            Grid.SetColumn(progressBar, 3);
            Grid.SetColumn(pauseButton, 4);
            Grid.SetColumn(stopButton, 5);
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
            grid.Children.Add(pauseButton);
            grid.Children.Add(stopButton);
            e.Children.Add(grid);
        }
    }
    public class Progress : IProgress<float>
    {
        private ProgressBar progressBar;
        private Label percentageLabel;
        public void Report(float value)
        {
            progressBar.Value = MathF.Round(value, 1);
            percentageLabel.Content = MathF.Round(value, 1) + "%";
        }
        public Progress(ProgressBar pb, Label pl) 
        {
            progressBar = pb;
            percentageLabel = pl;
        }
    }
    public partial class DownloaderWindow : Window
    {
        private int downloadNumber = 0;
        private Regex extensionExpression = new("(\\.)+(.{2,4})(?!.*\\1)");
        private ProgressBar progress;
        private Label percentage;
        public List<HttpClientPair> cancellationTokens = new List<HttpClientPair>();
        public DownloaderWindow()
        {
            InitializeComponent();
        }
        public void addDownload(string title, string url)
        {
            downloadNumber++;
            sp.Children.Add(new DownloaderTemplate(downloadNumber, title, url).e);
            startDownload();
        }
        public async void startDownload()
        {
            string url = string.Empty;
            string name = string.Empty;
            DockPanel lastChild = null;
            // Get the last children
            foreach (DockPanel dock in sp.Children)
            {
                lastChild = dock;
            }
            foreach (Grid grid in lastChild.Children)
            {
                // Get UIElements partecipating in the download
                UIElementCollection col = grid.Children;
                name = ((Label)col[0]).Content.ToString();
                url = ((Label)col[1]).Content.ToString();
                // Pause/Resume button: name and function
                Button pauseBtn = (Button)col[5];
                pauseBtn.Name = "PauseButton_" + downloadNumber.ToString();
                pauseBtn.Click += PauseButton_Click;
                // Stop button: name and function
                Button stopBtn = (Button)col[6];
                stopBtn.Name = "DownloadButton_" + downloadNumber.ToString();
                stopBtn.Click += StopButton_Click;

                progress = (ProgressBar)col[4];
                percentage = (Label)col[3];
                // Find a match in the regex expression
                Match match = extensionExpression.Match(url);
                // If match is found
                if (match.Success)
                {
                    bool isPaused = false;
                    Stream file = File.Open("C:\\Users\\alexi\\Desktop\\" + FilePathUtils.GetValidFilePath(name) + match.Value, FileMode.OpenOrCreate);
                    Stream response = await HttpClientSingleton.client.GetStreamAsync(url);
                    CancellationTokenSource cancellationToken = new CancellationTokenSource();
                    IsPaused p = new IsPaused(false);
                    cancellationTokens.Add(new HttpClientPair(cancellationToken, "DownloadButton_" + downloadNumber.ToString(), "PauseButton_" + downloadNumber.ToString(), p));
                    await HttpClientSingleton.DownloadAsync(HttpClientSingleton.client, url, file, new Progress(progress, percentage), cancellationToken.Token, p);
                }
            }
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
        public void StopButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(HttpClientPair pair in cancellationTokens)
            {

                if (pair.stopName.Equals(((Button)sender).Name))
                {
                    pair.cancellation.Cancel();
                    cancellationTokens.Remove(pair);
                    break;
                }
            }
        }
        public void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (HttpClientPair pair in cancellationTokens)
            {
                if (pair.pauseName.Equals(((Button)sender).Name))
                {
                    if (pair.isPaused.pause)
                    {
                        pair.isPaused.pause = false;
                    }
                    else
                    {
                        pair.isPaused.pause = true;
                    }
                }
            }
        }
    }
}