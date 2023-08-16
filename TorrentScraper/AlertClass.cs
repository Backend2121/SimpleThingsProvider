using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleThingsProvider
{
    public static class AlertClass
    {
        public static MessageBoxResult Alert(string messageBoxText, string caption, MessageBoxButton button , MessageBoxImage messageBoxImage)
        {
            Logger.Log("Alert sent: " + caption + "\nWith message: " +  messageBoxText, "Alert");
            return MessageBox.Show(messageBoxText, caption, button, messageBoxImage, MessageBoxResult.OK);
        }
    }
}