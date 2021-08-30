using Common.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Tesseract;

namespace Backend
{
    public class BackendController
    {
        private ScreenshotService screenshotService { get; } = new ScreenshotService();
        private ColorModificator colorModificator { get; } = new ColorModificator();

        private FuzzyMatcher fuzzyMatcher { get; } = new FuzzyMatcher();

        private BitmapAnalyzer bitmapAnalyzer { get; } = new BitmapAnalyzer();




        public Name Result { get; private set; }


        public bool ReadFromScreen()
        {
            Result = default;

            for (int i = 0; i < screenshotService.AmountOfPositionVariations; i++)
            {
                var bmp = screenshotService.TakeScreenshot(i);
                var colorModifications = colorModificator.GetAll(bmp);

                for (int j = 0; j < colorModifications.Count; j++)
                {
                    Log.Debug("OCR Attempt {0}-{1}", i, j);
                    bool success = bitmapAnalyzer.AnalyzeBmp(colorModifications[j]);
                    if (success)
                    {
                        Result = bitmapAnalyzer.Result;
                        return true;
                    } 
                }
            }

            Log.Information("No attempt led to a viable name. The engine will try to fit the parsed text to a viable name approximately.");

            var fuzzySuccess = fuzzyMatcher.Match(bitmapAnalyzer.ToFuzzyAnyalyze);
            if (fuzzySuccess)
            {
                Result = new Name(fuzzyMatcher.Result.First.Word, fuzzyMatcher.Result.Second.Word, fuzzyMatcher.Result.Third.Word);
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