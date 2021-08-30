using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using Tesseract;
using Serilog;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using System.IO;
using System.Text.RegularExpressions;
using Common;
using FuzzySharp;

namespace Backend
{
    public class OcrService
    {
        public ScreenshotService ScreenshotService { get; } = new ScreenshotService();
        public ColorModificator ColorModificator { get; } = new ColorModificator();
        public OcrCoreService OcrCoreService { get; } = new OcrCoreService(new TesseractEngine(@"./tessdata", "eng", EngineMode.Default));

        const float CONFIDENCE_SURE_LIMIT = 0.90f;
        const float CONFIDENCE_DUMP_LIMIT = 0.85f;


        List<List<string>> ToFuzzyAnyalyze = new List<List<string>>();
        (string, int)[] FinalWords = new (string, int)[3];

        private void InitializeService()
        {
            ToFuzzyAnyalyze = new List<List<string>>();
            FinalWords = new (string, int)[3]; //todo wrapyper
        }

        public bool ReadFromScreen(out string[] result)
        {
            InitializeService();


            List<string> ocrRawTexts = new List<string>();

            for (int i = 0; i < ScreenshotService.AmountOfPositionVariations; i++)
            {
                var bmp = ScreenshotService.TakeScreenshot(i);
                var colorModifications = ColorModificator.GetAll(bmp);

                for (int j = 0; j < colorModifications.Count; j++)
                {
                    Log.Debug("OCR Attempt {0}-{1}", i, j);

                    var ocrResult = OcrCoreService.DoOcr(bmp);
                    if (ocrResult.Confidence < CONFIDENCE_DUMP_LIMIT)
                    {
                        Log.Debug("Confidence too low. Dumping result.");
                        continue;
                    }

                    if (!ocrResult.HasText)
                    {
                        Log.Debug("No Text parsed. Dumping result.");
                        continue;
                    }

                    var garbageFiltered = FilterArtifacts(ocrResult.Text);
                    var agressiveFiltered = AggressiveArtifactFilter(ocrResult.Text);
                    var spaceInvariant = DetectMissingSpaces(ocrResult.Text);

                    bool success = TestForPerfectMatch(new List<List<string>>() { garbageFiltered, agressiveFiltered, spaceInvariant }, out var iterationresult1);
                    if (success)
                    {
                        result = iterationresult1.ToArray();
                        return true;
                    }

                    Log.Debug("No Perfect Match");

                    if (ocrResult.Confidence > CONFIDENCE_SURE_LIMIT)
                    {
                        Log.Warning("OCR was very confident, yet there was no name match. This can happen if Fall Guys added new name possiblities. Manual review recommended.");
                    }


                    this.ToFuzzyAnyalyze.Add(spaceInvariant);
                    this.ToFuzzyAnyalyze.Add(garbageFiltered);
                    this.ToFuzzyAnyalyze.Add(agressiveFiltered);
                }
            }

            Log.Information("No attempt led to a perfect match. The engine will try to fit the patterns approximately.");
            
            Console.WriteLine("Fuzzy Engine Started with the following texts avilable:");
            ToFuzzyAnyalyze.RemoveAll(entry => entry.Count() < 3);
            ToFuzzyAnyalyze = ToFuzzyAnyalyze.Distinct(new SequenceEqualsComparer()).ToList();
            ToFuzzyAnyalyze.ForEach(f => Console.WriteLine(string.Join(" ", f)));


            var magicSuccess = GoFuzzyMatching(out var fuzzyResult); //todo: reinsten out var und so ghetto...
            if (magicSuccess)
            {
                result = fuzzyResult.ToArray();
                return true;
            }


            result = new string[] { };
            return false;
        }

        private bool GoFuzzyMatching(out List<string> result)
        {
            foreach (var i in ToFuzzyAnyalyze.ToList())
            {
                if (i.Count > 3)
                {
                    var t = DetectMostLikelyPermutation(i);
                    ToFuzzyAnyalyze.Remove(i);
                    ToFuzzyAnyalyze.Add(t);
                }
                if (i.Count < 3)
                {
                    ToFuzzyAnyalyze.Remove(i);
                }
            }

            foreach (var i in ToFuzzyAnyalyze)
            {
                DoFuzzyComparison(i);
            }

            if (!FinalWords.ToList().All(entry => entry.Item2 != 0))
            {
            Log.Warning("Fuzzy matching failed.");
                result = null;
                return false;
            }



            result = FinalWords.Select(f => f.Item1).ToList();
            Log.Information("Fuzzy matching succeeded. The result is: {0}", FinalWords.Select(f => $"{f.Item1} ({f.Item2}%)").Aggregate((x, y) => $"{x} | {y}"));

            return true;
        }

        private void DoFuzzyComparison(List<string> words)
        {
            var nameOptions = new string[][] { PossibleNames.FirstNames(false), PossibleNames.SecondNames(false), PossibleNames.ThirdNames(false) };

            for (int i = 0; i < 3; i++)
            {
                var r = Process.ExtractOne(words[i], nameOptions[i], cutoff: 50);
                if (r != null)
                {
                    if (this.FinalWords[i].Item2 < r.Score)
                    {
                        Log.Debug("Fuzzy Matching Sucess: Position {0} - Input {1} - Matching {2} - Score {3}", i, words[i], r.Value, r.Score);
                        FinalWords[i].Item1 = r.Value;
                        FinalWords[i].Item2 = r.Score;
                    }
                } else
                {
                    Log.Debug("{0} could not be matched", words[i]);
                }

            }
        }

        private List<string> DetectMostLikelyPermutation(List<string> words)
        {
            Dictionary<int, int> scoreBoard = new Dictionary<int, int>();
            var subcollecitons = this.GetSubCollections(words);

            for (int i = 0; i < subcollecitons.Count; i++)
            {

                var result1 = Process.ExtractOne(subcollecitons[i][0], PossibleNames.FirstNames(false));
                var result2 = Process.ExtractOne(subcollecitons[i][1], PossibleNames.SecondNames(false));
                var result3 = Process.ExtractOne(subcollecitons[i][2], PossibleNames.ThirdNames(false));
                scoreBoard.Add(i, result1.Score + result2.Score + result3.Score);
            }

            var maxValue = scoreBoard.Values.Max();
            var maxIndex = scoreBoard.First(s => s.Value == maxValue).Key;
            var result = words.Skip(maxIndex).Take(3).ToList();
            Log.Debug("Best Fitting Triple of {0} is {1}", words, result);
            return result;
        }

        private bool TestForPerfectMatch(List<List<string>> input, out List<string> result)
        {

            List<List<string>> wordsOfLength3 = new List<List<string>>();

            // todo: hier subcolleciton logic machen.
            foreach (var entry in input)
            {
                if (entry.Count == 3)
                {
                    wordsOfLength3.Add(entry);
                }
                if (entry.Count > 3)
                {
                    var subs = GetSubCollections(entry);
                    wordsOfLength3.AddRange(subs);
                }
            }

            foreach (var entry in wordsOfLength3)
            {

                bool isViable = ValidateTripe(entry);
                if (isViable)
                {
                    Log.Information("Viable Text Found: '{0}'", entry);
                    result = entry;
                    return true;
                }

            }



            result = null;
            return false;

        }


        private List<string> FilterArtifacts(string input)
        {
            var result = input.Split(' ').Select(s => s.Trim()).Where(s => s.Length > 2).ToList();
            Log.Debug("Soft artifact filter delivered text to: {0}", result);
            return result;
        }

        private List<string> AggressiveArtifactFilter(string input)
        {
            string pattern = @"[A-Za-z]{3,}";

            Regex r = new Regex(pattern);
            var matches = r.Matches(input);

            List<string> words = new List<string>();
            foreach (Match m in matches)
            {
                words.Add(m.Value);
            }

            Log.Debug("Aggressive artifact filter delivered {0}", words);

            return words;
        }

        private List<string> DetectMissingSpaces(string input)
        {
            //string pattern = @"([A-Z][a-z]{2,}|VIP|MVP|IceCream)";  // das macht nun eig case invariatn
            string pattern = @"([A-Z][a-z]{2,})";

            Regex r = new Regex(pattern);
            var matches = r.Matches(input);

            List<string> words = new List<string>();
            foreach (Match m in matches)
            {
                words.Add(m.Value);
            }

            Log.Debug("Word-Extractor delivered {0}", words);

            return words;
        }


        private bool ValidateTripe(List<string> words)
        {
            if (words.Count != 3)
            {
                throw new Exception("words length is not 3.");
            }

            var p1 = PossibleNames.FirstNames(true);
            var p2 = PossibleNames.SecondNames(true);
            var p3 = PossibleNames.ThirdNames(true);

            // todo: würde heir bereits alles toupper haben wollen, damit das schonmal raus ist.

            var w1 = words[0].ToLower();
            var w2 = words[1].ToLower();
            var w3 = words[2].ToLower();

            return p1.Contains(w1) && p2.Contains(w2) && p3.Contains(w3);
        }

        private List<List<string>> GetSubCollections(List<string> words)
        {
            List<List<string>> result = new List<List<string>>();
            for (int i = 0; i <= words.Count - 3; i++)
            {
                var subcollection = words.Skip(i).Take(3);
                result.Add(subcollection.ToList());
            }
            return result;
        }

        private string GetScreenshotFileName(int attempt, int style)
        {
            var dateString = DateTime.Now.ToString("y-MM-dd_HH-mm-ss");
            return Path.Combine(DataStorageStuff.AppDir, "Screenshots", $"{dateString}_{attempt}_{style}.jpg");
        }
    }

    


}
