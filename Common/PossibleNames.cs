using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Backend
{
    public static class PossibleNames
    {
        public static string PossibilitiesDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                string directory = Path.GetDirectoryName(path);
                return Path.Combine(directory, "PossibleNames");
            }
        }

        private static string[] firsts;
        private static string[] firstsLower;
        private static string[] seconds;
        private static string[] secondsLower;
        private static string[] thirds;
        private static string[] thirdsLower;

        private static string[] GetAsLowerCase(string[] words)
        {
            return words.ToList().Select(s => s.ToLower()).ToArray();
        }

        public static string[] FirstNames(bool asLowerCase)
        {
            if (firsts == null)
            {
                var file = Path.Combine(PossibilitiesDirectory, "Possibilities1.txt");
                firsts = File.ReadAllLines(file);
                firstsLower = GetAsLowerCase(firsts);
            }
            if (asLowerCase)
            {
                return firstsLower;
            }
            else
            {
                return firsts;
            }
        }

        public static string[] SecondNames(bool asLowerCase)
        {
            if (seconds == null)
            {
                var file = Path.Combine(PossibilitiesDirectory, "Possibilities2.txt");
                seconds = File.ReadAllLines(file);
                secondsLower = GetAsLowerCase(seconds);
            }
            if (asLowerCase)
            {
                return secondsLower;
            }
            else
            {
                return seconds;
            }
        }

        public static string[] ThirdNames(bool asLowerCase)
        {
            if (thirds == null)
            {
                var file = Path.Combine(PossibilitiesDirectory, "Possibilities3.txt");
                thirds = File.ReadAllLines(file);
                thirdsLower = GetAsLowerCase(thirds);
            }
            if (asLowerCase)
            {
                return thirdsLower;
            }
            else
            {
                return thirds;
            }
        }
    }
}