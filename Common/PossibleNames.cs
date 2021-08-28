using Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using System.Threading.Tasks;
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
        private static string[] seconds;
        private static string[] thirds;



        public static string[] FirstNames()
        {
            if (firsts == null)
            {
                var file = Path.Combine(PossibilitiesDirectory, "Possibilities1.txt");
                firsts = File.ReadAllLines(file);
            }
            return firsts;
        }

        public static string[] SecondNames()
        {
            if (seconds == null)
            {
                var file = Path.Combine(PossibilitiesDirectory, "Possibilities2.txt");
                seconds = File.ReadAllLines(file);
            }
            return seconds;
        }

        public static string[] ThirdNames()
        {
            if (thirds == null)
            {
                var file = Path.Combine(PossibilitiesDirectory, "Possibilities3.txt");
                thirds = File.ReadAllLines(file);
            }
            return thirds;
        }
    }
}
