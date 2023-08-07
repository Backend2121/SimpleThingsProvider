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
        public Button stopButton { get; }
        public Button pauseButton { get; }
        public IsPaused isPaused { get; set; }
        public Label percentageLabel { get; }
        public HttpClientPair(CancellationTokenSource c, Button sn, Button pn, IsPaused p, Label pl) 
        {
            cancellation = c;
            stopButton = sn;
            pauseButton = pn;
            isPaused = p;
            percentageLabel = pl;
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
            columnDefinition1.Width = new GridLength(200, GridUnitType.Star);
            columnDefinition2.Width = new GridLength(100, GridUnitType.Star);
            columnDefinition3.Width = new GridLength(300, GridUnitType.Star);
            columnDefinition4.Width = new GridLength(50, GridUnitType.Star);
            columnDefinition5.Width = new GridLength(50, GridUnitType.Star);
            grid.ColumnDefinitions.Add(columnDefinition1);
            grid.ColumnDefinitions.Add(columnDefinition2);
            grid.ColumnDefinitions.Add(columnDefinition3);
            grid.ColumnDefinitions.Add(columnDefinition4);
            grid.ColumnDefinitions.Add(columnDefinition5);
            // The 3 labels
            Label titleLabel = new Label();
            Label percentageLabel = new Label();
            Label urlLabel = new Label();
            titleLabel.Content = t;
            titleLabel.Name = "titleLabel" + i.ToString();
            urlLabel.Content = u;
            urlLabel.Name = "urlLabel" + i.ToString();
            urlLabel.Visibility = Visibility.Collapsed;
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
            Grid.SetColumn(percentageLabel, 1);
            Grid.SetColumn(progressBar, 2);
            Grid.SetColumn(pauseButton, 3);
            Grid.SetColumn(stopButton, 4);
            // Setting the margins
            titleLabel.Margin = new Thickness(0, 0, 10, 0);
            percentageLabel.Margin = new Thickness(10, 0, 10, 0);
            progressBar.Margin = new Thickness(10, 5, 10, 5);
            stopButton.Margin = new Thickness(10, 0, 10, 0);
            grid.Margin = new Thickness(0, 0, 0, 10);
            grid.ShowGridLines = true;
            // Appending everything to the grid
            grid.Children.Add(titleLabel);
            grid.Children.Add(urlLabel);
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
        private int _downloadNumber = 0;
        private Regex _extensionExpression = new("(\\.)(jpg|JPG|gif|GIF|doc|DOC|pdf|PDF|zip|ZIP|rar|RAR|7z|7Z)$");
        private Dictionary<string, string> _settings = new Dictionary<string, string>();
        private string _configFileName = "Downloader_Config";
        public List<string> formats;

        private ProgressBar _progress;
        private Label _percentage;
        public List<HttpClientPair> cancellationTokens = new List<HttpClientPair>();
        public DownloaderWindow()
        {
            InitializeComponent();
        }
        public void addDownload(string title, string url)
        {
            _downloadNumber++;
            sp.Children.Add(new DownloaderTemplate(_downloadNumber, title, url).e);
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
                // Give a name to the percentage label
                Label percentageLabel = (Label)col[2];
                percentageLabel.Name = "PercentageLabel_" + _downloadNumber.ToString();
                // Pause/Resume button: name and function
                Button pauseBtn = (Button)col[4];
                pauseBtn.Name = "PauseButton_" + _downloadNumber.ToString();
                pauseBtn.Click += PauseButton_Click;
                // Stop button: name and function
                Button stopBtn = (Button)col[5];
                stopBtn.Name = "DownloadButton_" + _downloadNumber.ToString();
                stopBtn.Click += StopButton_Click;

                _progress = (ProgressBar)col[3];
                _percentage = (Label)col[2];
                // Find a match in the regex expression
                Match match = _extensionExpression.Match(url);
                Debug.WriteLine(match.Success);
                Debug.WriteLine(url);
                // If match is found
                if (match.Success)
                {
                    JsonSettings jsonSettings = new JsonSettings("Configs", _configFileName, _settings);
                    _settings = jsonSettings.loadFromJson();
                    bool isPaused = false;
                    Stream file;
                    try
                    {
                        if (_settings["downloadPath"].Equals(""))
                        {
                            file = File.Open(FilePathUtils.GetValidFilePath(name) + match.Value, FileMode.OpenOrCreate);
                        }
                        file = File.Open(_settings["downloadPath"] + "\\" + FilePathUtils.GetValidFilePath(name) + match.Value, FileMode.OpenOrCreate);
                    }
                    catch (KeyNotFoundException e)
                    {
                        file = File.Open(FilePathUtils.GetValidFilePath(name) + match.Value, FileMode.OpenOrCreate);
                    }

                    Stream response = await HttpClientSingleton.client.GetStreamAsync(url);
                    CancellationTokenSource cancellationToken = new CancellationTokenSource();
                    IsPaused p = new IsPaused(false);
                    cancellationTokens.Add(new HttpClientPair(cancellationToken, stopBtn, pauseBtn, p, percentageLabel));
                    await HttpClientSingleton.DownloadAsync(HttpClientSingleton.client, url, file, new Progress(_progress, _percentage), cancellationToken.Token, p);
                }
            }
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
                if (pair.stopButton.Name.Equals(((Button)sender).Name))
                {
                    pair.cancellation.Cancel();
                    cancellationTokens.Remove(pair);
                    Thread.Sleep(200);
                    if (pair.percentageLabel.Content.ToString().Contains("0%"))
                    {
                        pair.percentageLabel.Content = "Aborted";
                    }
                    pair.pauseButton.IsEnabled = false;                    
                    break;
                }
            }
        }
        public void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (HttpClientPair pair in cancellationTokens)
            {
                if (pair.pauseButton.Name.Equals(((Button)sender).Name))
                {
                    if (pair.isPaused.pause)
                    {
                        pair.isPaused.pause = false;
                        Thread.Sleep(200);
                        pair.percentageLabel.Content = "Resuming";
                        ((Button)sender).Content = "P";
                    }
                    else
                    {
                        pair.isPaused.pause = true;
                        Thread.Sleep(200);
                        pair.percentageLabel.Content = "Paused";
                        ((Button)sender).Content = "R";
                    }
                }
            }
        }
    }
}