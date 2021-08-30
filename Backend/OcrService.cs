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
        private FuzzyMatcher fuzzyMatcher { get; } = new FuzzyMatcher();

        const float CONFIDENCE_SURE_LIMIT = 0.90f;
        const float CONFIDENCE_DUMP_LIMIT = 0.30f;

        public List<WordProcessorResult> ToFuzzyAnyalyze { get; private set; }



        public bool ReadFromScreen(out Name result)
        {

            ToFuzzyAnyalyze = new List<WordProcessorResult>();

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

                    // We can think about skipping results with less than 3 words already here... right now they are kept but skiopped later and never used :O
                    // But it may make sense if you get two perfect words and only the third is lost.

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

            Log.Information("No attempt led to a viable name. The engine will try to fit the parsed text to a viable name approximately.");

            var fuzzySuccess = fuzzyMatcher.Match(ToFuzzyAnyalyze);
            if (fuzzySuccess)
            {
                result = new Name(fuzzyMatcher.Result.First.Word, fuzzyMatcher.Result.Second.Word, fuzzyMatcher.Result.Third.Word);
                return true;
            }

            return false;
        }


        private string GetScreenshotFileName(int attempt, int style)
        {
            var dateString = DateTime.Now.ToString("y-MM-dd_HH-mm-ss");
            return Path.Combine(DataStorageStuff.AppDir, "Screenshots", $"{dateString}_{attempt}_{style}.jpg");
        }
    }

    


}
