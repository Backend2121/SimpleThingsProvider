using SimpleThingsProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public DownloaderTemplate() 
        {
            e = new DockPanel();
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
            Label titleLabel = new Label();
            Label ETALabel = new Label();
            Label percentageLabel = new Label();
            titleLabel.Content = "Title";
            ETALabel.Content = "NULL";
            percentageLabel.Content = "%";
            ProgressBar progressBar = new ProgressBar();
            Button pauseButton = new Button();
            Button stopButton = new Button();
            pauseButton.Content = "Pause";
            stopButton.Content = "Cancel";
            Grid.SetColumn(titleLabel, 0);
            Grid.SetColumn(ETALabel, 1);
            Grid.SetColumn(percentageLabel, 2);
            Grid.SetColumn(progressBar, 3);
            Grid.SetColumn(pauseButton, 4);
            Grid.SetColumn(stopButton, 5);
            titleLabel.Margin = new Thickness(0, 0, 10, 0);
            ETALabel.Margin = new Thickness(10, 0, 10, 0);
            percentageLabel.Margin = new Thickness(10, 0, 10, 0);
            progressBar.Margin = new Thickness(10, 0, 10, 0);
            pauseButton.Margin = new Thickness(10, 0, 10, 0);
            stopButton.Margin = new Thickness(10, 0, 10, 0);
            grid.ShowGridLines = true;
            grid.Children.Add(titleLabel);
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
            for (int i = 0; i < 5; i++)
            {
                sp.Children.Add(new DownloaderTemplate().e);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Save current state
            Debug.WriteLine("Hello");
        }
    }
}