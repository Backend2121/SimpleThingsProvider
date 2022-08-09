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
        static string file = $"LOGS/{createLogFile()}";
        public static void Log(string logMessage, string whoami)
        {
            System.Diagnostics.Debug.WriteLine(file);
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
            if (!Directory.Exists("LOGS/"))
            {
                Directory.CreateDirectory("LOGS/");
            }
            
            return file;
        }
    }
}
