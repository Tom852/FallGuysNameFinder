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
using Common.Model;

namespace Backend
{
    public class OcrService
    {
        private ScreenshotService screenshotService { get; } = new ScreenshotService();
        private ColorModificator colorModificator { get; } = new ColorModificator();
        private OcrCoreService ocrCoreService { get; } = new OcrCoreService(new TesseractEngine(@"./tessdata", "eng", EngineMode.Default));
        private WordProcessor wordProcessor { get; } = new WordProcessor();
        private ViableNameDetector viableNameDetector { get; } = new ViableNameDetector();


        const float CONFIDENCE_SURE_LIMIT = 0.90f;
        const float CONFIDENCE_DUMP_LIMIT = 0.85f;


        List<WordProcessorResult> ToFuzzyAnyalyze = new List<WordProcessorResult>();
        (string, int)[] FinalWords = new (string, int)[3];

        private void InitializeService()
        {
            ToFuzzyAnyalyze = new List<WordProcessorResult>();
            FinalWords = new (string, int)[3]; //todo wrapyper
        }

        public bool ReadFromScreen(out Name result)
        {
            InitializeService();


            List<string> ocrRawTexts = new List<string>();

            for (int i = 0; i < screenshotService.AmountOfPositionVariations; i++)
            {
                var bmp = screenshotService.TakeScreenshot(i);
                var colorModifications = colorModificator.GetAll(bmp);

                for (int j = 0; j < colorModifications.Count; j++)
                {
                    Log.Debug("OCR Attempt {0}-{1}", i, j);

                    var ocrResult = ocrCoreService.DoOcr(bmp);
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

                    var garbageFiltered = wordProcessor.SoftArtifactFilter(ocrResult.Text);
                    var agressiveFiltered = wordProcessor.AggressiveArtifactFilter(ocrResult.Text);
                    var spaceInvariant = wordProcessor.WordExtractor(ocrResult.Text);

                    bool success = viableNameDetector.TestForViableName(garbageFiltered, agressiveFiltered, spaceInvariant);
                    if (success)
                    {
                        result = viableNameDetector.LastMatch;
                        return true;
                    }

                    Log.Debug("No viable name found.");

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




        private string GetScreenshotFileName(int attempt, int style)
        {
            var dateString = DateTime.Now.ToString("y-MM-dd_HH-mm-ss");
            return Path.Combine(DataStorageStuff.AppDir, "Screenshots", $"{dateString}_{attempt}_{style}.jpg");
        }
    }

    


}
