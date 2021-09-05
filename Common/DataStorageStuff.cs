using Common.Model;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common
{
    public static class DataStorageStuff
    {
        private const string RootFolderName = "FallGuysNameFinder";
        private const string ScreenshotFolderName = "Screenshots";
        private const string PatternFileName = "patterns.txt";
        private const string PoolFileName = "pool.json";
        private const string OptionFileName = "options.json";
        private const string StatsFileName = "stats.json";
        private const string LogFileName = "log.log";

        public static string AppDir { get; private set; }
        public static string ScreenshotDir { get; private set; }
        public static string PatternsFile { get; private set; }
        public static string OptionsFile { get; private set; }
        public static string LogFile { get; private set; }
        public static string StatsFile { get; private set; }
        public static string PoolFile { get; private set; }

        static DataStorageStuff()
        {
            var env = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppDir = Path.Combine(env, RootFolderName);
            ScreenshotDir = Path.Combine(env, RootFolderName, ScreenshotFolderName);
            PatternsFile = Path.Combine(env, RootFolderName, PatternFileName);
            PoolFile = Path.Combine(env, RootFolderName, PoolFileName);
            OptionsFile = Path.Combine(env, RootFolderName, OptionFileName);
            LogFile = Path.Combine(env, RootFolderName, LogFileName);
            StatsFile = Path.Combine(env, RootFolderName, StatsFileName);

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
            if (!File.Exists(PoolFile))
            {
                using (var stream = File.Create(PoolFile))
                using (StreamWriter sw = new StreamWriter(stream))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, new Pool());
                }
            }
            if (!File.Exists(StatsFile))
            {
                using (var stream = File.Create(StatsFile))
                using (StreamWriter sw = new StreamWriter(stream))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, new Statistics());
                }
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
        }

        public static List<Pattern> ReadPatterns()
        {
            var lines = File.ReadAllLines(PatternsFile);
            List<Pattern> result = new List<Pattern>();

            foreach (var line in lines)
            {
                result.Add(new Pattern(line));
            }
            return result;
        }

        public static void RemovePattern(int index)
        {
            var allLines = File.ReadAllLines(PatternsFile).ToList();
            allLines.RemoveAt(index);
            File.WriteAllLines(PatternsFile, allLines);
        }

        public static void AddPattern(Pattern p) => File.AppendAllLines(PatternsFile, new List<string>() { p.ToString() });

        public static void EditPattern(int index, string line)
        {
            var allLines = File.ReadAllLines(PatternsFile);
            allLines[index] = line;
            File.WriteAllLines(PatternsFile, allLines);
        }

        public static void EditPattern(int index, Pattern pattern) => EditPattern(index, pattern.ToString());

        public static Options GetOptions()
        {
            using (StreamReader sr = new StreamReader(OptionsFile))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                var result = (Options)serializer.Deserialize(reader, typeof(Options));
                return result ?? throw new IOException("Could not parse Options");
            }
        }

        public static void SaveOptions(Options o)
        {
            using (StreamWriter sw = new StreamWriter(OptionsFile))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, o);
            }
        }

        public static Statistics GetStats()
        {
            using (StreamReader sr = new StreamReader(StatsFile))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                var result = (Statistics)serializer.Deserialize(reader, typeof(Statistics));
                return result ?? throw new IOException("Could not parse Stats");
            }
        }

        public static void SaveStats(Statistics s)
        {
            using (StreamWriter sw = new StreamWriter(StatsFile))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, s);
            }
        }

        public static Pool GetStoredPool()
        {
            using (StreamReader sr = new StreamReader(PoolFile))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                var result = (Pool)serializer.Deserialize(reader, typeof(Pool));
                return result ?? throw new IOException("Could not parse Pool");
            }
        }

        public static void SavePool(Pool p)
        {
            using (StreamWriter sw = new StreamWriter(PoolFile))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(writer, p);
            }
        }
    }
}