using Common.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog;
using System.Threading.Tasks;

namespace Backend
{
    public class DataStorageStuff
    {
        const string RootFolderName = "FallGuysNameFinder";
        const string ScreenshotFolderName = "Screenshots";
        const string PatternFileName = "patterns.txt";
        const string OptionFileName = "options.json";

        public static string AppDir { get; private set; }
        public static string ScreenshotDir { get; private set; }
        public static string PatternsFile { get; private set; }
        public static string OptionsFile { get; private set; }

        public static string FirstNamesFile { get; private set; }
        public static string SecondNamesFile { get; private set; }
        public static string ThirdNamesFile { get; private set; }

        public static string LogNamesFile { get; private set; }


        static DataStorageStuff()
        {
            var env = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDir = Path.Combine(env, RootFolderName);
            ScreenshotDir = Path.Combine(env, RootFolderName, ScreenshotFolderName);
            PatternsFile = Path.Combine(env, RootFolderName, PatternFileName);
            OptionsFile = Path.Combine(env, RootFolderName, OptionFileName);
            FirstNamesFile = Path.Combine(env, RootFolderName, "Names1.txt");
            SecondNamesFile = Path.Combine(env, RootFolderName, "Names2.txt");
            ThirdNamesFile = Path.Combine(env, RootFolderName, "Names3.txt");
            LogNamesFile = Path.Combine(env, RootFolderName, "Log.log");

            if (!Directory.Exists(AppDir))
            {
                Directory.CreateDirectory(AppDir);
            }
            if (!Directory.Exists(ScreenshotDir))
            {
                Directory.CreateDirectory(ScreenshotDir);
            }
            if (!File.Exists(PatternsFile))
            {
                var stream = File.Create(PatternsFile);
                stream.Close();
            }
            if (!File.Exists(OptionsFile))
            {
                using (var stream = File.Create(OptionsFile))
                using (StreamWriter sw = new StreamWriter(stream))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, new Options());
                }
            }

            if (!File.Exists(FirstNamesFile))
            {
                var stream = File.Create(FirstNamesFile);
                stream.Close();
                File.WriteAllLines(FirstNamesFile, PossibleNamesDefaults.FirstNames);
            }

            if (!File.Exists(SecondNamesFile))
            {
                var stream = File.Create(SecondNamesFile);
                stream.Close();
                File.WriteAllLines(SecondNamesFile, PossibleNamesDefaults.SecondNames);
            }

            if (!File.Exists(ThirdNamesFile))
            {
                var stream = File.Create(ThirdNamesFile);
                stream.Close();
                File.WriteAllLines(ThirdNamesFile, PossibleNamesDefaults.ThirdNames);
            }
        }



        public List<Pattern> ReadPatterns()
        {
            var lines = File.ReadAllLines(PatternsFile);
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

        public void RemovePattern(int index)
        {
            var allLines = File.ReadAllLines(PatternsFile).ToList();
            allLines.RemoveAt(index);
            File.WriteAllLines(PatternsFile, allLines);
        }

        public void AddPattern(string line) => File.AppendAllLines(PatternsFile, new List<string>() { line });


        public void AddPattern(Pattern p) => AddPattern(p.ToString());

        public void EditPattern(int index, string line)
        {
            var allLines = File.ReadAllLines(PatternsFile);
            allLines[index] = line;
            File.WriteAllLines(PatternsFile, allLines);
        }

        public void EditPattern(int index, Pattern pattern) => EditPattern(index, pattern.ToString());


        public Options GetOptions()
        {

            using (StreamReader sr = new StreamReader(OptionsFile))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                var result = (Options)serializer.Deserialize(reader, typeof(Options));
                return result ?? throw new IOException("Could not parse Options");
            }
        }

        public void SaveOptions(Options o)
        {
            using (StreamWriter sw = new StreamWriter(OptionsFile))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, o);
            }
        }

        // todo: unbediungt cachen.
        public string[] GetNamePossibilities(int namePosition)
        {

            switch (namePosition)
            {
                case 1:
                    return File.ReadAllLines(FirstNamesFile);
                case 2:
                    return File.ReadAllLines(SecondNamesFile);
                case 3:
                    return File.ReadAllLines(ThirdNamesFile);
                default:
                    throw new Exception("Name Pos must be 1 for Firstname, 3 for Third Name, etc");

            }



        }
    }
}
