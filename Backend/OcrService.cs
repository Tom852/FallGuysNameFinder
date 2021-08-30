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

        const float CONFIDENCE_LIMIT = 0.85f;
        const int MONOCHROME_WHITE_BOUNDARY = 250;
        const int MONOCHROME_BRIGHT_BOUNDARY = 200;
        const int MONOCHROME_BACKGROUND_BOUNDARY = 150;

        const double xStartPercentage = 0.60;
        const double yStartPercentrage = 0.29;
        const double xEndPercentage = 0.745;
        const double yEndPercentage = 0.335;

        const decimal ratio16by9 = (decimal)16 / 9;

        int[,] variations = new int[,] {
            { 0,0,0,0, },
            { -10, -10, 20, 20,  },
            { 5, 5, 10, 10,  },
            { 10, 10, -20, -20,  },
            { 10, 0, -20, 0,  },
            { 20, 0, -40, 0,  },
            { -10, 0, 20, 0,  },
            { -20, -20, 40, 40,  },
            { -40, -40, 80, 80, },
        };

        List<List<string>> ToFuzzyAnyalyze = new List<List<string>>();
        (string, int)[] FinalWords = new (string, int)[3];

        const int amountOfColorModificaitons = 7;
        public bool ReadFromScreen(out string[] result)
        {
            ToFuzzyAnyalyze = new List<List<string>>();
            FinalWords = new (string, int)[3];

            Bitmap[,] bmps = new Bitmap[variations.GetLength(0), amountOfColorModificaitons];
            List<string> ocrRawTexts = new List<string>();

            for (int i = 0; i < variations.GetLength(0); i++)
            {
                for (int j = 0; j < amountOfColorModificaitons; j++)
                {
                    Log.Information("OCR Attempt {0}-{1}", i, j);

                    var bmp = TakeScerenshot(variations[i, 0], variations[i, 1], variations[i, 2], variations[i, 3]);

                    switch (j)
                    {
                        case 0:
                            ToMonochrome(bmp, MONOCHROME_WHITE_BOUNDARY, true);
                            break;
                        case 1:
                            ToMonochrome(bmp, MONOCHROME_BRIGHT_BOUNDARY, true);
                            break;
                        case 2:
                            ToMonochrome(bmp, MONOCHROME_BACKGROUND_BOUNDARY, true);
                            break;
                        case 3:
                            ToMonochrome(bmp, MONOCHROME_WHITE_BOUNDARY, false);
                            break;
                        case 4:
                            ToMonochrome(bmp, MONOCHROME_BRIGHT_BOUNDARY, false);
                            break;
                        case 5:
                            ToGrayScale(bmp);
                            break;
                        case 6:
                            break;
                    }
                    bmp.Save(GetScreenshotFile(i, j), ImageFormat.Jpeg); // temporary :)


                    DoOcr(bmp, out var textRaw, out var confidence);
                    if (confidence < 0.4)
                    {
                        Log.Debug("Confidence too low. Dumping result.");
                        continue;
                    }

                    if (textRaw.Trim() == string.Empty)
                    {
                        Log.Debug("No Text parsed. Dumping result.");
                        continue;
                    }

                    var garbageFiltered = FilterArtifacts(textRaw);
                    var agressiveFiltered = AggressiveArtifactFilter(textRaw);
                    var spaceInvariant = DetectMissingSpaces(textRaw);

                    bool success = TestForPerfectMatch(new List<List<string>>() { garbageFiltered, agressiveFiltered, spaceInvariant }, out var iterationresult1);
                    if (success)
                    {
                        result = iterationresult1.ToArray();
                        return true;
                    }

                    Log.Debug("No Perfect Match");

                    if (confidence > 0.85)
                    {
                        Log.Warning("OCR was very confident, yet there was no name match. This can happen if Fall Guys added new name possiblities. Manual review recommended.");
                    }


                    bmps[i, j] = bmp;
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



        private Bitmap TakeScerenshot(int startVariationX = 0, int startVariationY = 0, int sizeVariationX = 0, int sizeVariationY = 0)
        {


            var screenshotArea = this.GetScreenshotArea();

            var sizeToCapture = new Size((int)screenshotArea.Width + sizeVariationX, (int)screenshotArea.Height + sizeVariationY);



            var bmpScreenshot = new Bitmap(sizeToCapture.Width,
                               sizeToCapture.Height,
                               PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);


            gfxScreenshot.CopyFromScreen((int)screenshotArea.Left + startVariationX,
                                         (int)screenshotArea.Top + startVariationY,
                                0,
                                0,
                                sizeToCapture,
                                CopyPixelOperation.SourceCopy);

            return bmpScreenshot;
        }

        private WindowPosition GetScreenshotArea()
        {
            var windowPosition = FgWindowAccess.GetPositionShit();

            decimal ratio = windowPosition.Width / (decimal)windowPosition.Height;


            WindowPosition result = new WindowPosition(0, 0, 0, 0);

            if (Math.Abs((ratio - ratio16by9)) < 0.001m)
            {
                // Full Screen 16:9 Aspect Ratio
                double relativeStartX = windowPosition.Width * xStartPercentage;
                double relativeStartY = windowPosition.Height * yStartPercentrage;
                result.Left = (int)relativeStartX + windowPosition.Left;
                result.Top = (int)relativeStartY + windowPosition.Top;

                result.Right = (int)(windowPosition.Width * xEndPercentage + windowPosition.Left);
                result.Bottom = (int)(windowPosition.Height * yEndPercentage + windowPosition.Top);

                Log.Debug("Detected Full Screen 16:9");
            }
            else if (ratio == 1.6m)
            {
                //Full Screen 16:10 (because i have that xD)
                if (windowPosition.Height != 1200)
                {
                    throw new Exception("16:10 Resolution is not supported unless it's 1920x1200 Full Screen. Try running FG in windowed mode with 16:9.");
                }
                Log.Debug("Detected Full Screen 1920x1200");


                double relativeStartX = windowPosition.Width * xStartPercentage;
                double relativeStartY = 1080 * yStartPercentrage;
                result.Left = (int)relativeStartX + windowPosition.Left;
                result.Top = (int)relativeStartY + windowPosition.Top + 60;

                result.Right = (int)(windowPosition.Width * xEndPercentage + windowPosition.Left);
                result.Bottom = (int)(1080 * yEndPercentage) + 60 + windowPosition.Top;
            }
            else
            {
                var effectiveWindowWidth = windowPosition.Width - 2 * 8;
                var effectiveWindowHeight = windowPosition.Height - 31 - 8;
                var effectiveWindowLeft = windowPosition.Left + 8;
                var effectiveWindowTop = windowPosition.Top + 31;

                decimal ratioWhenWindowed = effectiveWindowWidth / (decimal)effectiveWindowHeight;

                if (!((ratioWhenWindowed - ratio16by9) < 0.001m))
                {
                    throw new Exception("Unknown aspect ratio or programmatic error. Try to run 16:9 fullscreen. That usually works. 16:9 windowed should work too.");
                }

                Log.Debug("Detected Windowed Fall Guys. Effective Width: {0}; Hieght: {1}", effectiveWindowWidth, effectiveWindowHeight);

                // Windowed case: Adds 30px top, and 8 all other sides.
                double relativeStartX = effectiveWindowWidth * xStartPercentage;
                double relativeStartY = effectiveWindowHeight * yStartPercentrage;
                result.Left = (int)relativeStartX + effectiveWindowLeft;
                result.Top = (int)relativeStartY + effectiveWindowTop;

                result.Right = (int)(effectiveWindowWidth * xEndPercentage + effectiveWindowLeft);
                result.Bottom = (int)(effectiveWindowHeight * yEndPercentage + effectiveWindowTop);
            }
            return result;
        }

        private string GetScreenshotFile(int attempt, int style)
        {
            var dateString = DateTime.Now.ToString("y-MM-dd_HH-mm-ss");
            return Path.Combine(DataStorageStuff.AppDir, "Screenshots", $"{dateString}_{attempt}_{style}.jpg");
        }

        private void DoOcr(Bitmap b, out string textRaw, out float confidence)
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {

                using (Page page = engine.Process(b, PageSegMode.SingleBlock)) // todo: single line?
                {
                    textRaw = page.GetText();
                    confidence = page.GetMeanConfidence();
                    Log.Information("With confidence {confidence}, the following text was parsed: '{text}'", confidence, string.IsNullOrWhiteSpace(textRaw) ? "[No Text]" : textRaw.Trim());
                }
            }
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


        public void ToMonochrome(Bitmap Bmp, int whiteBoundary, bool inverted)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    if (rgb > whiteBoundary)
                    {
                        if (inverted)
                        {
                            Bmp.SetPixel(x, y, Color.Black);
                        } else
                        {
                            Bmp.SetPixel(x, y, Color.White);
                        }
                    }
                    else
                    {
                        if (inverted)
                        {
                            Bmp.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            Bmp.SetPixel(x, y, Color.Black);
                        }
                    }
                }
        }

        public void ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
        }

        public void ToGrayScaleInverted(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    rgb = 255 - rgb;
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
        }
    }

    internal class SequenceEqualsComparer : IEqualityComparer<List<string>>
    {
        public bool Equals(List<string> x, List<string> y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<string> obj)
        {
           return obj.Aggregate(0, (x, y) => x = x + y.GetHashCode());
        }
    }
}
