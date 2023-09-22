using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleThingsProvider
{
    public static class FilePathUtils
    {
        public static string GetValidFilePath(string inputString)
        {
            // Rimuovi i caratteri non validi per i nomi dei file su Windows
            string cleanString = RemoveInvalidPathChars(inputString);

            // Sostituisci spazi con trattini bassi
            cleanString = cleanString.Replace(' ', '_');

            // Riduci la lunghezza massima del percorso (ad esempio, 260 caratteri per Windows)
            int maxPathLength = 260;
            if (cleanString.Length > maxPathLength)
            {
                cleanString = cleanString.Substring(0, maxPathLength);
            }

            return cleanString;
        }

        private static string RemoveInvalidPathChars(string input)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()));
            string invalidCharsPattern = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(input, invalidCharsPattern, "_");
        }
    }

}
