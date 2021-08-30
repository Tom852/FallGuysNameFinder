using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Tesseract;

namespace Backend
{
    public class BitmapAnalyzer
    {

        private const float CONFIDENCE_SURE_LIMIT = 0.90f;
        private const float CONFIDENCE_DUMP_LIMIT = 0.30f;

        public Name Result { get; private set; }
        public List<WordProcessorResult> ToFuzzyAnyalyze { get; private set; } = new List<WordProcessorResult>();

        private OcrCoreService ocrCoreService { get; } = new OcrCoreService(new TesseractEngine(@"./tessdata", "eng", EngineMode.Default));
        private WordProcessor wordProcessor { get; } = new WordProcessor();
        private ViableNameDetector viableNameDetector { get; } = new ViableNameDetector();

        public bool AnalyzeBmp(Bitmap bmp)
        {

            var ocrResult = ocrCoreService.DoOcr(bmp);
            if (ocrResult.Confidence < CONFIDENCE_DUMP_LIMIT)
            {
                Log.Debug("Confidence too low. Dumping result.");
                return false;
            }

            // We can think about skipping results with less than 3 words already here... right now they are kept but skiopped later and never used :O
            // But it may make sense if you get two perfect words and only the third is lost.

            if (!ocrResult.HasText)
            {
                Log.Debug("No Text parsed. Dumping result.");
                return false;
            }

            var garbageFiltered = wordProcessor.SoftArtifactFilter(ocrResult.Text);
            var agressiveFiltered = wordProcessor.AggressiveArtifactFilter(ocrResult.Text);
            var spaceInvariant = wordProcessor.WordExtractor(ocrResult.Text);

            bool success = viableNameDetector.TestForViableName(garbageFiltered, agressiveFiltered, spaceInvariant);
            if (success)
            {
                Result = viableNameDetector.LastMatch;
                return true;
            }

            Log.Debug("No viable name found.");

            if (ocrResult.Confidence > CONFIDENCE_SURE_LIMIT)
            {
                Log.Warning("OCR was very confident, yet there was no name match. This can happen if Fall Guys added new name possiblities. Manual review recommended.");
            }

            ToFuzzyAnyalyze.Add(spaceInvariant);
            ToFuzzyAnyalyze.Add(garbageFiltered);
            ToFuzzyAnyalyze.Add(agressiveFiltered);

            return false;
        }
    }
}
