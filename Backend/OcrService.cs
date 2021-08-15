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

namespace Backend
{
    public class OcrService
    {

        public string DoOcr(Bitmap b)
        {
            using (var engine = new TesseractEngine(@"./tessdata_fast-master", "eng", EngineMode.Default))
            {

                using (Page page = engine.Process(b, PageSegMode.SingleBlock))
                {
                    var text = page.GetText();
                    Log.Information("Identified Text: {0} with confidence {1}", text, page.GetMeanConfidence());
                    return text;
                }
            }
        }

    }
}
