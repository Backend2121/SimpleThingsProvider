using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleThingsProvider
{
    static class Logger
    {
        static string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/SimpleThingsProvider_logs/{createLogFile()}";
        public static void Log(string logMessage, string whoami)
        {
            using (StreamWriter w = File.AppendText(file))
            {
                w.Write("\r\nLog Entry : ");
                w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                w.WriteLine($"  :{whoami}");
                w.WriteLine($"  :{logMessage}");
                w.WriteLine("-------------------------------");
            }
        }
        private static string createLogFile()
        {
            var file = $"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}".Replace(":", "_") + ".log";
            file = file.Replace(" ", "_");
            System.Diagnostics.Debug.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/SimpleThingsProvider_logs/"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/SimpleThingsProvider_logs/");
            }
            
            return file;
        }
    }
}
