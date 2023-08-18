using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace SimpleThingsProvider
{
    public static class AlertClass
    {
        public static MessageBoxResult Alert(string messageBoxText, string caption, MessageBoxButton button , MessageBoxImage messageBoxImage)
        {
            Logger.Log("Alert sent: " + caption + "\nWith message: " +  messageBoxText, "Alert");
            return MessageBox.Show(messageBoxText, caption, button, messageBoxImage, MessageBoxResult.OK);
        }

        public class CustomMessageBox : Window
        {
            public CustomMessageBox(string title, string message)
            {
                Title = title;
                Width = 300;
                Height = 150;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                WindowStyle = WindowStyle.ToolWindow;
                ResizeMode = ResizeMode.NoResize;
                SolidColorBrush color;
                if (Settings.Default.MainTheme == "Light")
                {
                    color = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    color = new SolidColorBrush(Colors.White);
                }

                var textBlock = new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(20),
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = color,
                };
                Content = textBlock;
            }
        }
    }
}