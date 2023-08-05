using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleThingsProvider
{
    public class JsonSettings
    {
        private string directory;
        private string file;
        private Dictionary<string, string> settings;
        public JsonSettings(string where, string fileName, Dictionary<string, string> pairs)
        {
            directory = where;
            file = fileName;
            settings = pairs;
        }
        public void saveToJson()
        {
            Debug.WriteLine(JsonSerializer.Serialize(settings));
            File.WriteAllText(directory + "\\" + file, JsonSerializer.Serialize(settings));
        }
        public Dictionary<string, string> loadFromJson()
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(directory + "\\" + file));
            }
            catch (DirectoryNotFoundException e)
            {
                Directory.CreateDirectory(directory);
                File.Create(directory + "\\" + file).Close();
                return new Dictionary<string, string>();
            }
            catch(FileNotFoundException e)
            {
                Directory.CreateDirectory(directory);
                File.Create(directory + "\\" + file).Close();
                return new Dictionary<string, string>();
            }
            catch(JsonException e)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
