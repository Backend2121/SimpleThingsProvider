using MahApps.Metro.Controls;
using SimpleThingsProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DownloaderWindow
{
    public class DownloaderTemplate
    {
        public DockPanel e;
        public DownloaderTemplate(int i) 
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
            titleLabel.Content = "Title";
            titleLabel.Name = "titleLabel" + i.ToString();
            urlLabel.Content = "URL";
            urlLabel.Name = "urlLabel" + i.ToString();
            urlLabel.Visibility = Visibility.Collapsed;
            ETALabel.Content = "NULL";
            percentageLabel.Content = "%";
            // Progress bar
            ProgressBar progressBar = new ProgressBar();
            // Pause/Resume and Stop buttons
            Button pauseButton = new Button();
            Button stopButton = new Button();
            pauseButton.Content = "P";
            stopButton.Content = "S";
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
            pauseButton.Margin = new Thickness(10, 0, 10, 0);
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
    public partial class DownloaderWindow : Window
    {
        public DownloaderWindow()
        {
            InitializeComponent();
            int childCounter = 0;
            string json = "{\"Downloads\":{";
            // Read from file how many downloads needs to be instantiated
            
            for (int i = 0; i < 5; i++)
            {
                sp.Children.Add(new DownloaderTemplate(i).e);
            }
            // Serializer
            foreach (DockPanel dock in sp.Children)
            {
                foreach (Grid grid in dock.Children)
                {
                    UIElementCollection elements = grid.Children;
                    Label titlelabel =  (Label)elements[0];
                    Label urllabel = (Label)elements[1];
                    // Serialize child element
                    json += "\"" + childCounter.ToString() + "\":{\"url\":\"" + urllabel.Content + "\",\"name\":\"" + titlelabel.Content + "\"},";
                }
                childCounter++;
            }
            json = json.Remove(json.Length - 1);
            json += "}}";
            File.WriteAllText(".\\downloads.json", json);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Save current state
            foreach (DockPanel dock in sp.Children)
            {
                foreach (Grid grid in dock.Children)
                    // Serialize child element
                    Debug.WriteLine(JsonSerializer.Serialize(grid));
            }
            Debug.WriteLine("Hello");
        }
    }
}