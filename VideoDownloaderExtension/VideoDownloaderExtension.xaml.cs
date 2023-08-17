using SimpleThingsProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace VideoDownloaderExtension
{
    public class Pair
    {
        public CancellationTokenSource _cancellationTokenSource { get; }
        public string _name;
        public Pair(string n, CancellationTokenSource t) 
        {
            _cancellationTokenSource = t;
            _name = n;
        }
    }
    public class DownloaderTemplate
    {
        public DockPanel e;
        public ProgressBar progressBar;
        public Button stopButton;
        public TextBlock percentageTB;
        public Label titleLabel;
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
            columnDefinition1.Width = new GridLength(200, GridUnitType.Star);
            columnDefinition2.Width = new GridLength(50, GridUnitType.Star);
            columnDefinition3.Width = new GridLength(50, GridUnitType.Star);
            columnDefinition4.Width = new GridLength(300, GridUnitType.Star);
            columnDefinition5.Width = new GridLength(50, GridUnitType.Star);
            columnDefinition6.Width = new GridLength(50, GridUnitType.Star);
            grid.ColumnDefinitions.Add(columnDefinition1);
            grid.ColumnDefinitions.Add(columnDefinition2);
            grid.ColumnDefinitions.Add(columnDefinition3);
            grid.ColumnDefinitions.Add(columnDefinition4);
            grid.ColumnDefinitions.Add(columnDefinition5);
            grid.ColumnDefinitions.Add(columnDefinition6);
            // The 3 labels
            titleLabel = new Label();
            percentageTB = new TextBlock();
            Label urlLabel = new Label();
            titleLabel.Content = t;
            titleLabel.Name = "titleLabel" + i.ToString();
            urlLabel.Content = u;
            urlLabel.Name = "urlLabel" + i.ToString();
            urlLabel.Visibility = Visibility.Collapsed;
            // Progress bar
            progressBar = new ProgressBar();
            progressBar.Name = "progressBar" + i.ToString();
            // Stop button
            stopButton = new Button();
            stopButton.Content = "S";
            stopButton.Name = "stopButton" + i.ToString();

            // Set the elements to the grid
            Grid.SetColumn(titleLabel, 0);
            Grid.SetColumn(urlLabel, 0);
            Grid.SetColumn(percentageTB, 1);
            Grid.SetColumn(progressBar, 3);
            Grid.SetColumn(stopButton, 4);
            // Setting the margins
            titleLabel.Margin = new Thickness(0, 0, 10, 0);
            percentageTB.Margin = new Thickness(10, 0, 10, 0);
            progressBar.Margin = new Thickness(10, 5, 10, 5);
            stopButton.Margin = new Thickness(10, 0, 10, 0);
            grid.Margin = new Thickness(0, 0, 0, 10);
            grid.ShowGridLines = true;
            // Appending everything to the grid
            grid.Children.Add(titleLabel);
            grid.Children.Add(urlLabel);
            grid.Children.Add(percentageTB);
            grid.Children.Add(progressBar);
            grid.Children.Add(stopButton);
            e.Children.Add(grid);
            // Style the textbox
            // Imposta l'allineamento al centro
            percentageTB.TextAlignment = TextAlignment.Center;
            percentageTB.VerticalAlignment = VerticalAlignment.Center;

            // Imposta il colore del testo su bianco
            percentageTB.Foreground = Brushes.White;
            // Set Up percentage label's binding
            Binding binding = new Binding();
            binding.Path = new PropertyPath("Value");
            binding.Source = progressBar; // Imposta la ProgressBar come origine del binding
            PercentageConverter percentageConverter = new PercentageConverter();
            binding.Converter = percentageConverter;
            percentageTB.SetBinding(TextBlock.TextProperty, binding);
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class VideoDownloaderWindow : Window
    {
        private YoutubeDL _ytdl = new YoutubeDL();
        private RunResult<YoutubeDLSharp.Metadata.VideoData> runResult;
        private int downloadNumber = 0;
        private List<Pair> pairs = new List<Pair>();
        private Dictionary<string, string> formats = new Dictionary<string, string>();
        private List<string> formatStrings = new List<string>();
        private Dictionary<string, string> settings = new Dictionary<string, string>();
        private string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string tempDownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public VideoDownloaderWindow()
        {
            InitializeComponent();
            FormatMenu.ItemsSource = formats;
            foreach (string key in formats.Keys)
            {
                formatStrings.Add(key);
            }
            FormatMenu.ItemsSource = formatStrings;
            JsonSettings jsonSettings = new JsonSettings("Configs", "VDW_Config.json", settings);
            settings = jsonSettings.loadFromJson();
            if (settings.Count <= 0)
            {
                settings.Add("downloadPath", downloadPath);
                settings.Add("tempDownloadPath", tempDownloadPath);
            }
            downloadPath = settings["downloadPath"];
            tempDownloadPath = settings["tempDownloadPath"];
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }
        private async void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            if (runResult.Data != null)
            {
                // Specify the format immediatly
                var currentFormat = "best";

                if (FormatMenu.SelectedValue != null)
                {
                    currentFormat = formats[FormatMenu.SelectedValue.ToString()];
                }
                JsonSettings jsonSettings = new JsonSettings("Configs", "VDW_Config.json", settings);
                settings = jsonSettings.loadFromJson();
                // Add new download bar to the listview
                downloadNumber++;
                DownloaderTemplate template = new DownloaderTemplate(downloadNumber, LinkBox.Text, "Title");
                sp.Children.Add(template.e);
                // Add click event to stop button
                template.stopButton.Click += StopButton_Click;
                // set the path of yt-dlp and FFmpeg if they're not in PATH or current directory
                // Check if YtDl and FFmpeg are in the CWD
                // Always needed
                if (!File.Exists("yt-dlp.exe"))
                {
                    MessageBoxResult choice = AlertClass.Alert("You are missing a necessary component: Yt-Dlp.exe \n Would you like to download it?", "VD_Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (choice == MessageBoxResult.Yes)
                    {
                        await Utils.DownloadYtDlp();
                        AlertClass.Alert("Done", "VD_Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                // Needed only for reconversions and post-processing operations
                if (currentFormat == "aac" || currentFormat == "alac" || currentFormat == "flac" || currentFormat == "m4a" || currentFormat == "mp3" || currentFormat == "opus" || currentFormat == "vorbis" || currentFormat == "wav")
                {
                    if (!File.Exists("ffmpeg.exe"))
                    {
                        MessageBoxResult choice = AlertClass.Alert("You are missing a necessary component: ffmpeg.exe \n Would you like to download it?", "VD_Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (choice == MessageBoxResult.Yes)
                        {
                            await Utils.DownloadFFmpeg();
                            AlertClass.Alert("Done", "VD_Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    if (!File.Exists("ffprobe.exe"))
                    {
                        MessageBoxResult choice = AlertClass.Alert("You are missing a necessary component: ffprobe.exe \n Would you like to download it?", "VD_Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (choice == MessageBoxResult.Yes)
                        {
                            await Utils.DownloadFFprobe();
                            AlertClass.Alert("Done", "VD_Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                _ytdl.YoutubeDLPath = "yt-dlp.exe";
                _ytdl.FFmpegPath = "ffmpeg.exe";

                var options = new OptionSet()
                {
                    NoContinue = true,
                    RestrictFilenames = true,
                    Format = currentFormat,
                };
                options.AddCustomOption<string>("-P", settings["downloadPath"]);
                options.AddCustomOption<string>("-P", "temp:" + settings["tempDownloadPath"]);
                // --paths temp:name_of_tmp_dir
                // Progress handler with a callback that updates a progress bar
                Progress<DownloadProgress> progress = new Progress<DownloadProgress>(p => template.progressBar.Value = p.Progress * 100);
                template.titleLabel.Content = runResult.Data.Title + " [" + currentFormat + "]";
                options.AddCustomOption<string>("-o", runResult.Data.Title + "_" + currentFormat + ".%(ext)s");
                // Cancellation token source used for cancelling the download
                CancellationTokenSource cts = new CancellationTokenSource();
                pairs.Add(new Pair("stopButton" + downloadNumber.ToString(), cts));
                template.progressBar.Value = 0;
                try
                {
                    if (currentFormat == "aac" || currentFormat == "flac" || currentFormat == "m4a" || currentFormat == "mp3" || currentFormat == "opus" || currentFormat == "vorbis" || currentFormat == "wav")
                    {
                        Debug.WriteLine("Conversion to " + currentFormat + " needed");
                        options.Format = "best";
                        // Here we need to run the conversion option
                        options.ExtractAudio = true;
                        switch (currentFormat)
                        {
                            case "aac":
                                options.AudioFormat = AudioConversionFormat.Aac;
                                break;
                            case "flac":
                                options.AudioFormat = AudioConversionFormat.Flac;
                                break;
                            case "m4a":
                                options.AudioFormat = AudioConversionFormat.M4a;
                                break;
                            case "mp3":
                                options.AudioFormat = AudioConversionFormat.Mp3;
                                break;
                            case "opus":
                                options.AudioFormat = AudioConversionFormat.Opus;
                                break;
                            case "vorbis":
                                options.AudioFormat = AudioConversionFormat.Vorbis;
                                break;
                            case "wav":
                                options.AudioFormat = AudioConversionFormat.Wav;
                                break;
                        }
                    }
                    RunResult<string> r = await _ytdl.RunVideoDownload(LinkBox.Text, progress: progress, ct: cts.Token, overrideOptions: options);
                    foreach (string error in r.ErrorOutput)
                    {
                        Debug.WriteLine(error);
                    }
                    
                    template.progressBar.Value = 100;
                }
                catch (TaskCanceledException ex)
                {
                    template.progressBar.Value = 0;
                    template.percentageTB.Text = "Stopped";
                }
                template.stopButton.IsEnabled = false;
            }
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(Pair pair in pairs)
            {
                if (pair._name.Equals(((Button)sender).Name))
                {
                    pair._cancellationTokenSource.Cancel();
                }
            }
        }

        private async void LinkBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Do this only if the textbox contains something that resemples a link using REGEX
            runResult = await _ytdl.RunVideoDataFetch(LinkBox.Text);
            formats.Clear();
            if (runResult.Data != null)
            {
                foreach (FormatData item in runResult.Data.Formats)
                {
                    formats.Add(item.Format, item.FormatId);
                }
                formats.Add("Best Audio+Video", "best");
                formats.Add("Best Video", "bestvideo");
                formats.Add("Best Audio", "bestaudio");
                formats.Add("Worst Audio+Video", "worst");
                formats.Add("Worst Video", "worstvideo");
                formats.Add("Worst Audio", "worstaudio");
                formats.Add("Convert to 'aac'", "aac");
                formats.Add("Convert to 'flac'", "flac");
                formats.Add("Convert to 'm4a'", "m4a");
                formats.Add("Convert to 'mp3'", "mp3");
                formats.Add("Convert to 'opus'", "opus");
                formats.Add("Convert to 'vorbis'", "vorbis");
                formats.Add("Convert to 'wav'", "wav");
            }
            FormatMenu.ItemsSource = formats.Keys;
        }
    }
}