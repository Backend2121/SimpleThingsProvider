﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VideoDownloaderExtension;
using System.Windows.Forms;
using Label = System.Windows.Controls.Label;
using System.Security;

namespace SimpleThingsProvider
{
    partial class VideoDownloaderExtension : IExtension
    {
        public string name { get { return "Youtube Video Downloader"; } set { } }
        public Window extensionWindow { get; set; }
        private Window dw;
        private System.Windows.Controls.TextBox downloadPathTB;
        private System.Windows.Controls.TextBox tempDownloadPathTB;
        public VideoDownloaderExtension()
        {
            extensionWindow = new VideoDownloaderWindow();
            dw = (VideoDownloaderWindow)extensionWindow;
        }

        public void disableButton(Label outputLabel)
        {
            return;
        }

        public void enableButton(Label putLabel)
        {
            return;
        }

        public List<UIElement> getElements(System.Windows.Controls.ListView lv, Label ol)
        { 
            List<UIElement> list = new List<UIElement>();
            return list;
        }

        public Window getExtensionWindow()
        {
            if (dw == null)
            {
                dw = new VideoDownloaderWindow();
            }
            return dw;
        }

        public void showWindow()
        {
            // Show the downloader window
            dw.Show();
            dw.Focus();
        }

        public bool startFunction(object[] args)
        {
            throw new NotImplementedException();
        }

        public List<DockPanel> getSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            JsonSettings jsonSettings = new JsonSettings("Configs", "VDW_Config.json", settings);
            settings = jsonSettings.loadFromJson();
            List<DockPanel> dockPanels = new List<DockPanel>();
            DockPanel row1 = new DockPanel();
            DockPanel row2 = new DockPanel();
            Grid grid1 = new Grid();
            Grid grid2 = new Grid();
            ColumnDefinition column1Definition1 = new ColumnDefinition();
            ColumnDefinition column1Definition2 = new ColumnDefinition();
            ColumnDefinition column1Definition3 = new ColumnDefinition();
            column1Definition1.Width = new GridLength(1, GridUnitType.Star);
            column1Definition2.Width = new GridLength(3, GridUnitType.Star);
            column1Definition3.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinition column2Definition1 = new ColumnDefinition();
            ColumnDefinition column2Definition2 = new ColumnDefinition();
            ColumnDefinition column2Definition3 = new ColumnDefinition();
            column2Definition1.Width = new GridLength(1, GridUnitType.Star);
            column2Definition2.Width = new GridLength(3, GridUnitType.Star);
            column2Definition3.Width = new GridLength(1, GridUnitType.Star);
            grid1.ColumnDefinitions.Add(column1Definition1);
            grid1.ColumnDefinitions.Add(column1Definition2);
            grid1.ColumnDefinitions.Add(column1Definition3);
            grid2.ColumnDefinitions.Add(column2Definition1);
            grid2.ColumnDefinitions.Add(column2Definition2);
            grid2.ColumnDefinitions.Add(column2Definition3);

            // Labels
            Label downloadPathLabel = new Label();
            Label tempDownloadPathLabel = new Label();
            downloadPathLabel.Content = "Download path: ";
            downloadPathLabel.Margin = new Thickness(10, 10, 10, 10);
            tempDownloadPathLabel.Content = "Temp files location: ";
            tempDownloadPathLabel.Margin = new Thickness(10, 10, 10, 10);

            // TextBoxes
            downloadPathTB = new System.Windows.Controls.TextBox();
            downloadPathTB.IsReadOnly = true;            
            downloadPathTB.Margin = new Thickness(10, 10, 10, 10);
            tempDownloadPathTB = new System.Windows.Controls.TextBox();
            tempDownloadPathTB.IsReadOnly = true;
            tempDownloadPathTB.Margin = new Thickness(10, 10, 10, 10);
            try
            {
                downloadPathTB.Text = settings["downloadPath"];
                tempDownloadPathTB.Text = settings["tempDownloadPath"];
            }
            catch { }

            // Path Pickers
            System.Windows.Controls.Button downloadPathPicker = new System.Windows.Controls.Button();
            downloadPathPicker.Name = "downloadPathPicker";
            downloadPathPicker.Content = "...";
            downloadPathPicker.Click += PathPicker_Click;
            downloadPathPicker.Margin = new Thickness(10, 10, 10, 10);
            System.Windows.Controls.Button tempDownloadPathPicker = new System.Windows.Controls.Button();
            tempDownloadPathPicker.Name = "tempDownloadPathPicker";
            tempDownloadPathPicker.Content = "...";
            tempDownloadPathPicker.Click += PathPicker_Click;
            tempDownloadPathPicker.Margin = new Thickness(10, 10, 10, 10);
            // Set to columns
            Grid.SetColumn(downloadPathLabel, 0);
            Grid.SetColumn(tempDownloadPathLabel, 0);
            Grid.SetColumn(downloadPathTB, 1);
            Grid.SetColumn(tempDownloadPathTB, 1);
            Grid.SetColumn(downloadPathPicker, 2);
            Grid.SetColumn(tempDownloadPathPicker, 2);
            // Add childrens to grid
            grid1.Children.Add(downloadPathLabel);
            grid2.Children.Add(tempDownloadPathLabel);
            grid1.Children.Add(downloadPathTB);
            grid2.Children.Add(tempDownloadPathTB);
            grid1.Children.Add(downloadPathPicker);
            grid2.Children.Add(tempDownloadPathPicker);
            row1.Children.Add(grid1);
            row2.Children.Add(grid2);
            dockPanels.Add(row1);
            dockPanels.Add(row2);
            return dockPanels;
        }

        private void PathPicker_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBroserDialog = new FolderBrowserDialog();
            folderBroserDialog.Description = "Choose a path";

            // Mostra il selettore di percorsi e verifica se l'utente ha selezionato un percorso valido
            DialogResult result = folderBroserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (((System.Windows.Controls.Button)sender).Name.Equals("downloadPathPicker"))
                {
                    downloadPathTB.Text = folderBroserDialog.SelectedPath;
                }
                else
                {
                    tempDownloadPathTB.Text = folderBroserDialog.SelectedPath;
                }
            }
        }

        public void saveSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings.Add("downloadPath", downloadPathTB.Text);
            settings.Add("tempDownloadPath", tempDownloadPathTB.Text);
            JsonSettings jsonSettings = new JsonSettings("Configs", "VDW_Config.json", settings);
            jsonSettings.saveToJson();
        }
    }
}