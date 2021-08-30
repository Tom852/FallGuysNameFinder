using Backend.Model;
using Serilog;
using System.Drawing;
using Tesseract;

namespace Backend
{
    public class OcrCoreService
    {
        private readonly TesseractEngine engine;

        public OcrCoreService(TesseractEngine engine)
        {
            this.engine = engine;
        }

        public OcrResult DoOcr(Bitmap b)
        {
            using (Page page = engine.Process(b, PageSegMode.SingleBlock)) // todo: single line?
            {
                var textRaw = page.GetText().Trim();
                var confidence = page.GetMeanConfidence();
                var result = new OcrResult(textRaw, confidence);
                Log.Information("With confidence {confidence}, the following text was parsed: '{text}'", result.Confidence, result.HasText ? result.Text : "[No Text]");
                return result;
            }
        }
    }
}