using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace SimpleThingsProvider
{
    public static class BannedWords
    {
        public static List<string> nsfwWords = JsonSerializer.Deserialize<List<string>>(File.ReadAllText("data/NSFWWords.json"));
    }
}