using Common;
using Serilog;
using System;
using System.Drawing;
using System.Drawing.Imaging;

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

        private const decimal ratio16by9 = (decimal)16 / 9;
        private const decimal ratio21by9 = 2.389m; // 2.370 / 2.3888 / 2.4 depending on res :O

        private readonly int[,] variations = new int[,] {
            // left, top, width, height
            { 0, 0, 0, 0 },

            { 0, -5, 0, 10 },  // increase height - for two-lined names
            { 0, -10, 0, 20 },  // increase height - for two-lined names

            { 5, 0, -5, 0 },   // decrease width from left in case artifact is in screen
            { 10, 0, -10, 0 },   // same

            { 4, 2, -8, -4},  // narrow area slightly in case too much is captured, e.g. for 21:9 variance

            { -5, -5, 10, 10 },  // increase area incase we are off, e.g. for 21:9 variance
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

            var screenshotArea = this.GetScreenshotArea();

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


        private WindowPosition GetScreenshotArea()
        {
            var windowPosition = FgWindowAccess.GetPositionShit();

            decimal ratio = windowPosition.Width / (decimal)windowPosition.Height;

            WindowPosition result = default;

            // todo: someone should probably refactor this
            if (DecimalIsEqual(ratio, ratio16by9))
            {
                // Full Screen 16:9 Aspect Ratio
                double relativeStartX = windowPosition.Width * xStartPercentage16by9;
                double relativeStartY = windowPosition.Height * yStartPercentrage;
                result.Left = (int)relativeStartX + windowPosition.Left;
                result.Top = (int)relativeStartY + windowPosition.Top;
                
                result.Right = (int)(windowPosition.Width * xEndPercentage16by9 + windowPosition.Left);
                result.Bottom = (int)(windowPosition.Height * yEndPercentage + windowPosition.Top);

                Log.Debug("Detected full screen 16:9");
            }
            else if (ratio == 1.6m)
            {
                //Full Screen 16:10 (because i have that xD)
                if (windowPosition.Height != 1200)
                {
                    throw new Exception("16:10 Resolution is not supported unless it's 1920x1200 full screen. Try running FG in windowed mode with 16:9. If many people require this, e.g. for Steam Deck, send me an Email :P.");
                }
                Log.Debug("Detected full screen 1920x1200");

                double relativeStartX = windowPosition.Width * xStartPercentage16by9;
                double relativeStartY = 1080 * yStartPercentrage;
                result.Left = (int)relativeStartX + windowPosition.Left;
                result.Top = (int)relativeStartY + windowPosition.Top + 60;

                result.Right = (int)(windowPosition.Width * xEndPercentage16by9 + windowPosition.Left);
                result.Bottom = (int)(1080 * yEndPercentage) + 60 + windowPosition.Top;
            }
            else if (DecimalIsEqual(ratio, ratio21by9, 0.03m))
            {
                // Full Screen "21:9" Aspect Ratio
                double relativeStartX = windowPosition.Width * xStartPercentage21by9;
                double relativeStartY = windowPosition.Height * yStartPercentrage;
                result.Left = (int)relativeStartX + windowPosition.Left;
                result.Top = (int)relativeStartY + windowPosition.Top;

                result.Right = (int)(windowPosition.Width * xEndPercentage21by9 + windowPosition.Left);
                result.Bottom = (int)(windowPosition.Height * yEndPercentage + windowPosition.Top);

                Log.Debug("Detected full screen 21:9");
            }
            else
            {
                // assuming windowed
                // Windowed case: Adds 30px top, and 8 all other sides.
                var effectiveWindowWidth = windowPosition.Width - 2 * 8;
                var effectiveWindowHeight = windowPosition.Height - 31 - 8;
                var effectiveWindowLeft = windowPosition.Left + 8;
                var effectiveWindowTop = windowPosition.Top + 31;

                decimal ratioWhenWindowed = effectiveWindowWidth / (decimal)effectiveWindowHeight;

                if (!DecimalIsEqual(ratio, ratio16by9))
                {
                    throw new Exception($"Aspect ratio not supported. Only 16:9 full screen, 21:9 fullscreen, 1920x1200 fullscree, 16:9 windowed are supported. Use windowed 16:9 for now. --> Please get in touch with me (see about section) so I can implement your resolution. Please send me a fullscreen screenshot of your profile page and the following data: Width:{windowPosition.Width} Height:{windowPosition.Height} Left:{windowPosition.Left} Top:{windowPosition.Top} Right:{windowPosition.Right} Bot:{windowPosition.Bottom}");
                }

                Log.Debug("Detected windowed Fall Guys. Effective width: {0}. Effective height: {1}", effectiveWindowWidth, effectiveWindowHeight);

                double relativeStartX = effectiveWindowWidth * xStartPercentage16by9;
                double relativeStartY = effectiveWindowHeight * yStartPercentrage;
                result.Left = (int)relativeStartX + effectiveWindowLeft;
                result.Top = (int)relativeStartY + effectiveWindowTop;

                result.Right = (int)(effectiveWindowWidth * xEndPercentage16by9 + effectiveWindowLeft);
                result.Bottom = (int)(effectiveWindowHeight * yEndPercentage + effectiveWindowTop);
            }
            return result;
        }

        private bool DecimalIsEqual(decimal a, decimal b, decimal tolerance = 0.001m) => Math.Abs(a - b) < tolerance;

    }
}