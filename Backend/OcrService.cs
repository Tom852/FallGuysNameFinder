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

namespace Backend
{
    public class OcrService
    {
        const float CONFIDENCE_LIMIT = 0.85f;

        const double xStart = 0.60;
        const double yStart = 0.29;
        const double xEnd = 0.745;
        const double yEnd = 0.335;

        int[,] variations = new int[,] {
            { 0,0,0,0, },
            { -10, -10, 20, 20,  },
            { 5, 5, -5, -5,  },
            { 10, 10, -20, -20,  },
            { 10, 0, -20, 0,  },
            { 20, 0, -40, 0,  },

            { -10, 0, 20, 0,  },
            { -20, -20, 40, 40,  },
            { -30, -30, 60, 40,  },
            { -40, -40, 80, 80, },
        };

        public bool ReadFromScreen(out string[] result)
        {
            for (int i = 0; i < variations.GetLength(0); i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var bmp = TakeScerenshot(variations[i, 0], variations[i, 1], variations[i, 2], variations[i, 3]);

                    switch (j)
                    {
                        case 0:
                            ToMonochromeInverted(bmp);
                            break;
                        case 1:
                            ToMonochrome(bmp);
                            break;
                        case 2:
                            ToGrayScale(bmp);
                            break;
                        case 3:
                            break;
                    }

                    Log.Debug("Parsing Attempt {i}-{j}", i, j);
                    var ocrSuccess = DoOcr(bmp, out var text);
                    var refined = Refine(text);
                    bool isViable = Validate(refined);

                    if (isViable)
                    {
                        Log.Debug("Parsing successful. Viable Name detected.");
                        result = refined.ToArray();
                        return true;
                    }
                    if (ocrSuccess)
                    {
                        var p1 = PossibleNames.FirstNames();
                        var p2 = PossibleNames.SecondNames();
                        var p3 = PossibleNames.ThirdNames();

                        var s1 = p1.Contains(refined[0]);
                        var s2 = p2.Contains(refined[1]);
                        var s3 = p3.Contains(refined[2]);

                        Console.WriteLine();
                        Console.WriteLine("DEBUG INFORMATION");
                        Log.Debug("Words 1 - Success {0} - Word {1}", s1, refined[0]);
                        Log.Debug("Words 2 - Success {0} - Word {1}", s2, refined[1]);
                        Log.Debug("Words 2 - Success {0} - Word {1}", s3, refined[2]);
                        throw new Exception("OCR is very confident, but name seems not viable. Is a name possibility not within the possibility collection? Was the name refined in a wrong way?");
                    }
                    
                    Log.Information("Parsing failed.");
                    if (j == 3)
                    {
                        Log.Information("Screenshot saved.");
                        bmp.Save(GetScreenshotFile(i, j), ImageFormat.Jpeg);
                    }
                }


            }
            Log.Warning("Could not parse text.");
            result = new string[] { };
            return false;
        }



        private Bitmap TakeScerenshot(int startVariationX = 0, int startVariationY = 0, int sizeVariationX = 0, int sizeVariationY = 0)
        {

            var screenSize = Screen.PrimaryScreen.Bounds.Size;

            double startX = screenSize.Width*xStart + startVariationX;
            double startY = screenSize.Height*yStart + startVariationY;


            double sizeX = screenSize.Width * (xEnd - xStart) + sizeVariationX;
            double sizeY = screenSize.Height * (yEnd - yStart) + sizeVariationY;

            // 1920 x 1200 special case :)
            if (screenSize.Height == 1200)
            {
                startY = 1080 * yStart + 60 + startVariationY;
                sizeY = 1080 * (yEnd - yStart) + sizeVariationY;
            }

            var sizeToCapture = new Size((int)sizeX, (int)sizeY);



            var bmpScreenshot = new Bitmap(sizeToCapture.Width,
                               sizeToCapture.Height,
                               PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);


            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X + (int)startX,
                                        Screen.PrimaryScreen.Bounds.Y + (int)startY,
                                0,
                                0,
                                sizeToCapture,
                                CopyPixelOperation.SourceCopy);
            
            return bmpScreenshot;
        }

        private string GetScreenshotFile(int attempt, int style)
        {
            var dateString = DateTime.Now.ToString("y-MM-dd_HH-mm-ss");
            return Path.Combine(DataStorageStuff.AppDir, "Screenshots", $"{dateString}_{attempt}_{style}.jpg");
        }

        private bool DoOcr(Bitmap b, out string text)
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {

                using (Page page = engine.Process(b, PageSegMode.SingleBlock)) // todo: single line?
                {
                    text = page.GetText();
                    var confidence = page.GetMeanConfidence();
                    Log.Information("With confidence {confidence}, the following text was parsed: {text}", confidence, text.Trim() );
                    return confidence > CONFIDENCE_LIMIT;
                }
            }
        }

        private List<string> Refine(string input)
        {
            string pattern = @"([A-Z][a-z]{2,}|VIP|MVP)";
            Regex r = new Regex(pattern);
            var matches = r.Matches(input);

            List<string> words = new List<string>();
            foreach (Match m in matches)
            {
                words.Add(m.Value);
            }

            Log.Information("Text was refined to {0}", string.Join(" ", words));

            return words;
        }

        private bool Validate(List<string> words)
        {
            if (words.Count < 3)
            {
                return false;
            }
            if (words.Count == 3)
            {
                return ValidateTripe(words.ToArray());
            }

            for (int i = 0; i < words.Count - 3; i++)
            {
                var subcollection = words.Skip(i).Take(3);
                var isGood = ValidateTripe(subcollection.ToArray());
                if (isGood)
                {
                    Log.Information("Viable Subcollection detected");
                    words = subcollection.ToList();
                    return true;
                }
            }
            return false;

        }

        private bool ValidateTripe(string[] words)
        {
            if (words.Length!=3)
            {
                throw new Exception("words length is not 3.");
            }
            var p1 = PossibleNames.FirstNames();
            var p2 = PossibleNames.SecondNames();
            var p3 = PossibleNames.ThirdNames();

            // todo: würde heir bereits alles toupper haben wollen, damit das schonmal raus ist.

            return p1.Contains(words[0]) && p2.Contains(words[1]) && p3.Contains(words[2]);
        }


        public void ToMonochrome(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    if (rgb > 200)
                    {
                        Bmp.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        Bmp.SetPixel(x, y, Color.Black);
                    }
                }
        }
        public void ToMonochromeInverted(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
                    if (rgb > 200)
                    {
                        Bmp.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        Bmp.SetPixel(x, y, Color.White);
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
}
