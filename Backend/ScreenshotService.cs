using Common;
using Serilog;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Backend
{
    public class ScreenshotService
    {
        private const double yStartPercentrage = 0.29;
        private const double yEndPercentage = 0.335;


        private const double xStartPercentage16by9 = 0.60;
        private const double xEndPercentage16by9 = 0.745;

        private const double xStartPercentage21by9 = 0.58;
        private const double xEndPercentage21by9 = 0.68;

        private const decimal ratio16by9 = (decimal)16 / 9;   // 1.7777777
        private const decimal ratioTolerance16by9 = 0.01m;

        private const decimal ratio21by9 = 2.389m; // 2.370 / 2.3888 / 2.4 depending on res :O
        private const decimal ratioTolerance21by9 = 0.03m;



        private readonly int[,] variations = new int[,] {
            // left, top, width, height
            { 0, 0, 0, 0 },

            { 0, -5, 0, 10 },  // increase height - for two-lined names
            { 0, -10, 0, 20 },  // increase height - for two-lined names

            { 5, 0, -5, 0 },   // decrease width from left in case artifact is in screen
            { 10, 0, -10, 0 },   // same

            { 4, 2, -8, -4},  // narrow area slightly in case too much is captured, e.g. for 21:9 variance

            { -5, -5, 10, 10 },  // increase area in case we are off, e.g. for 21:9 variance
            { -10, -10, 20, 20 },  // going more radical to get more data for fuzzy matching
            { -25, -25, 50, 50 },
            { -50, -50, 100, 100},
        };

        public int AmountOfPositionVariations => variations.GetLength(0);

        public Bitmap TakeScreenshot(int variationIndex)
        {
            int startVariationX = variations[variationIndex, 0];
            int startVariationY = variations[variationIndex, 1];
            int sizeVariationX = variations[variationIndex, 2];
            int sizeVariationY = variations[variationIndex, 3];

            var screenshotArea = this.EvaluateScreenshotArea();

            var sizeToCapture = new Size((int)screenshotArea.Width + sizeVariationX, (int)screenshotArea.Height + sizeVariationY);

            var bmpScreenshot = new Bitmap(sizeToCapture.Width,
                               sizeToCapture.Height,
                               PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            gfxScreenshot.CopyFromScreen(screenshotArea.Left + startVariationX,
                                         screenshotArea.Top + startVariationY,
                                0,
                                0,
                                sizeToCapture,
                                CopyPixelOperation.SourceCopy);

            return bmpScreenshot;
        }

        public string GetScreenshotFileName(string tag)
        {
            var dateString = DateTime.Now.ToString("y-MM-dd_HH-mm-ss");
            return Path.Combine(DataStorageStuff.AppDir, "Screenshots", $"{dateString}_{tag}.jpg");
        }


        private WindowPosition EvaluateScreenshotArea()
        {
            var rawWindowPosition = FgWindowAccess.GetPositionShit();

            decimal ratio = rawWindowPosition.Width / (decimal)rawWindowPosition.Height;


            if (IsWindowed16by9(rawWindowPosition, out var effectiveWindowedModePosition))
            {
                Log.Debug("Detected windowed Fall Guys.");
                return GetScreenshotAreaFor16by9Ratio(effectiveWindowedModePosition);
            }


            if (Is16by9orNarrower(rawWindowPosition))
            {
                var effectivePosition = GetEffectiveWindowPositionFor16by9orNarrower(rawWindowPosition);
                Log.Debug("Detected full screen 16:9 or narrower.");
                return GetScreenshotAreaFor16by9Ratio(effectivePosition);
            }


            // todo: derive general rule for wider ratios, but I have no data from e.g. 32:9
            if (Math.Abs(ratio - ratio21by9) < ratioTolerance21by9)
            {
                Log.Debug("Detected full screen 21:9");
                return GetScreenshotAreaFor21by9Ratio(rawWindowPosition);
            }

            throw new Exception($"Aspect ratio not supported. Only 16:9 full screen, ratios narrower than 16:9 (e.g. 3:2 or 16:10) full screen, 21:9 full screen, 16:9 windowed are supported. Use windowed 16:9 for now. --> Please get in touch with me (see about section) so I can implement your resolution. Please send me a full screen screenshot of your profile page and the following data: Width:{rawWindowPosition.Width} Height:{rawWindowPosition.Height} Left:{rawWindowPosition.Left} Top:{rawWindowPosition.Top} Right:{rawWindowPosition.Right} Bot:{rawWindowPosition.Bottom}");

        }

        private bool Is16by9orNarrower(WindowPosition windowPosition)
        {
            Log.Debug("Temp Log: 16:9 Or Narrower Check Result: {0}", windowPosition.Width / windowPosition.Height <= ratio16by9 + ratioTolerance16by9);
            return windowPosition.Width / windowPosition.Height <= ratio16by9 + ratioTolerance16by9;
        }

        private WindowPosition GetEffectiveWindowPositionFor16by9orNarrower(WindowPosition windowPosition)
        {
            var blackBarPixelsForNarrowRatios = windowPosition.Height - windowPosition.Width / ratio16by9;

            var effectiveWindowLeft = windowPosition.Left;
            var effectiveWindowRight = windowPosition.Right;

            var effectiveWindowTop = windowPosition.Top + (int)blackBarPixelsForNarrowRatios / 2;
            var effectiveWindowBot = windowPosition.Bottom - (int)blackBarPixelsForNarrowRatios / 2;

            var result = new WindowPosition(effectiveWindowLeft, effectiveWindowRight, effectiveWindowTop, effectiveWindowBot);


            return result;

        }

        private bool IsWindowed16by9(WindowPosition windowPosition, out WindowPosition effectivePosition)
        {

            // Windowed case: Adds 31px top, and 8 all other sides for the window border, mouse overhead and title bar.
            var effectiveWindowLeft = windowPosition.Left + 8;
            var effectiveWindowTop = windowPosition.Top + 31;
            var effectiveWindowRight = windowPosition.Right - 8;
            var effectiveWindowBot = windowPosition.Bottom - 8;

            effectivePosition = new WindowPosition(effectiveWindowLeft, effectiveWindowRight, effectiveWindowTop, effectiveWindowBot);
            var ratioWhenWindowed = effectivePosition.Width / (decimal)effectivePosition.Height;

            return Math.Abs(ratioWhenWindowed - ratio16by9) < ratioTolerance16by9;
        }

        private WindowPosition GetScreenshotAreaFor16by9Ratio(WindowPosition effective16by9Position)
        {
            WindowPosition result = default;

            double relativeStartX = effective16by9Position.Width * xStartPercentage16by9;
            double relativeStartY = effective16by9Position.Height * yStartPercentrage;
            result.Left = (int)relativeStartX + effective16by9Position.Left;
            result.Top = (int)relativeStartY + effective16by9Position.Top;

            result.Right = (int)(effective16by9Position.Width * xEndPercentage16by9 + effective16by9Position.Left);
            result.Bottom = (int)(effective16by9Position.Height * yEndPercentage + effective16by9Position.Top);
            return result;
        }

        private WindowPosition GetScreenshotAreaFor21by9Ratio(WindowPosition windowPosition)
        {
            WindowPosition result = default;

            double relativeStartX = windowPosition.Width * xStartPercentage21by9;
            double relativeStartY = windowPosition.Height * yStartPercentrage;
            result.Left = (int)relativeStartX + windowPosition.Left;
            result.Top = (int)relativeStartY + windowPosition.Top;

            result.Right = (int)(windowPosition.Width * xEndPercentage21by9 + windowPosition.Left);
            result.Bottom = (int)(windowPosition.Height * yEndPercentage + windowPosition.Top);
            return result;
        }

        public void SaveFullScreenDebugScreenshot(string tag)
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                           Screen.PrimaryScreen.Bounds.Height,
                                           PixelFormat.Format32bppArgb);

            var gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                                        Screen.PrimaryScreen.Bounds.Y,
                                        0,
                                        0,
                                        Screen.PrimaryScreen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);

            bmpScreenshot.Save(this.GetScreenshotFileName(tag), ImageFormat.Jpeg);

            Log.Debug("Debug Screenshot Saved at " + DataStorageStuff.AppDir);
        }
    }
}