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

namespace Backend
{
    public class OcrService
    {
        const float CONFIDENCE_LIMIT = 0.7f; // rather take it low here, and very words by list.

        int[,] variations = new int[,] {
            { 0,0,0,0, },
            { -10, -10, 20, 20,  },
            { -10, 0, 20, 0,  },
            { -20, -20, 40, 40,  },
            { -30, -30, 60, 40,  },
            { -40, -40, 80, 80, },
            { -50, -50, 100, 100, },
            { -100, 0, 100, 0, },
            { -100, 0, 200, 0, },
            { -200, 0, 200, 0, },
            { -200, 0, 300, 0, },
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
                    var success = DoOcr(bmp, out var text);

                    // toido: coinfidence würd ich gar nicht merh anschauen tbh.
                    if (success)
                    {
                        Log.Information("Parsing successful.");
                        result = FilterGargbae(text);
                        return true;
                    }
                    else
                    {
                        Log.Information("Parsing failed. Screenshot will be saved.");
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

            int startX = 1160 + startVariationX;
            int startY = 320 + startVariationY;

            if (screenSize.Width != 1920)
            {
                throw new Exception("Only 1920x1080 and 1920x1200 supported.");
            }

            if (screenSize.Height == 1200)
            {
                startY += 60;
            }

            int widthX = 270 + sizeVariationX;
            int widthY = 35 + sizeVariationY;

            var sizeToCapture = new Size(widthX, widthY);



            var bmpScreenshot = new Bitmap(sizeToCapture.Width,
                               sizeToCapture.Height,
                               PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);


            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X + startX,
                                        Screen.PrimaryScreen.Bounds.Y + startY,
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

        private string[] FilterGargbae(string input)
        {

            return input.Split(' ').Where(s => s.Length >= 3).Select(s => s.Trim()).ToArray();
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
