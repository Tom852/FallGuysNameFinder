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
