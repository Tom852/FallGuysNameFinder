using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class DataStorageStuff
    {
        const string Appname = "FallGuysNameFinder";
        const string ScreenshotFolderName = "Screenshots";
        const string FileName = "names.txt";

        public static string AppDir { get; private set; }
        public static string ScreenshotDir { get; private set; }
        public static string NamesFile { get; private set; }

        static DataStorageStuff()
        {
            var env = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDir = Path.Combine(env, Appname);
            ScreenshotDir = Path.Combine(env, Appname, ScreenshotFolderName);
            NamesFile = Path.Combine(env, Appname, FileName);

            if (!Directory.Exists(AppDir))
            {
                Directory.CreateDirectory(AppDir);
            }
            if (!Directory.Exists(ScreenshotDir))
            {
                Directory.CreateDirectory(ScreenshotDir);
            }
            if (!File.Exists(NamesFile))
            {
                var stream = File.Create(NamesFile);
                stream.Close();
            }
        }

        public List<Pattern> Read()
        {
            var lines = File.ReadAllLines(NamesFile);
            List<Pattern> result = new List<Pattern>();

            foreach (var line in lines)
            {
                try
                {
                    result.Add(new Pattern(line));
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error while reading names-file. Item Skipped.");
                }
            }
            return result;
        }

        public void AddLine(string line)
        {
            File.AppendAllLines(NamesFile, new List<string>() { line });
        }
    }
}
