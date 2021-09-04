using Common;
using Serilog;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Backend
{
    public class ScreenshotService
    {
        private const double xStartPercentage = 0.60;
        private const double yStartPercentrage = 0.29;
        private const double xEndPercentage = 0.745;
        private const double yEndPercentage = 0.335;

        private const decimal ratio16by9 = (decimal)16 / 9;
        private const decimal ratio32by9 = (decimal)32 / 9;

        private readonly int[,] variations = new int[,] {
            { 0,0,0,0, },
            { -10, -10, 20, 20,  },
            { 5, 5, 10, 10,  },
            { 10, 10, -20, -20,  },
            { 10, 0, -20, 0,  },
            { 20, 0, -40, 0,  },
            { -10, 0, 20, 0,  },
            { -20, -20, 40, 40,  },
            { -40, -40, 80, 80, },
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

            gfxScreenshot.CopyFromScreen((int)screenshotArea.Left + startVariationX,
                                         (int)screenshotArea.Top + startVariationY,
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

            if (Math.Abs((ratio - ratio16by9)) < 0.001m)
            {
                // Full Screen 16:9 Aspect Ratio
                double relativeStartX = windowPosition.Width * xStartPercentage;
                double relativeStartY = windowPosition.Height * yStartPercentrage;
                result.Left = (int)relativeStartX + windowPosition.Left;
                result.Top = (int)relativeStartY + windowPosition.Top;

                result.Right = (int)(windowPosition.Width * xEndPercentage + windowPosition.Left);
                result.Bottom = (int)(windowPosition.Height * yEndPercentage + windowPosition.Top);

                Log.Debug("Detected Full Screen 16:9");
            }
            else if (ratio == 1.6m)
            {
                //Full Screen 16:10 (because i have that xD)
                if (windowPosition.Height != 1200)
                {
                    throw new Exception("16:10 Resolution is not supported unless it's 1920x1200 full screen. Try running FG in windowed mode with 16:9. If many people require this, e.g. for Steam Deck, send me an Email :P.");
                }
                Log.Debug("Detected Full Screen 1920x1200");

                double relativeStartX = windowPosition.Width * xStartPercentage;
                double relativeStartY = 1080 * yStartPercentrage;
                result.Left = (int)relativeStartX + windowPosition.Left;
                result.Top = (int)relativeStartY + windowPosition.Top + 60;

                result.Right = (int)(windowPosition.Width * xEndPercentage + windowPosition.Left);
                result.Bottom = (int)(1080 * yEndPercentage) + 60 + windowPosition.Top;
            }
            else if (Math.Abs((ratio - ratio32by9)) < 0.001m)
            {
                throw new Exception($"Ultrawide not yet supported. Use windowed 16:9 for now. --> Please get in touch with me (see about section) so I can implement this. I don't have enough data :O. Please send me a fullscreen screenshot of your profile page and the following data: Width:{windowPosition.Width} Height:{windowPosition.Height} Left:{windowPosition.Left} Top:{windowPosition.Top} Right:{windowPosition.Right} Bot:{windowPosition.Bottom}");
            }
            else
            {
                var effectiveWindowWidth = windowPosition.Width - 2 * 8;
                var effectiveWindowHeight = windowPosition.Height - 31 - 8;
                var effectiveWindowLeft = windowPosition.Left + 8;
                var effectiveWindowTop = windowPosition.Top + 31;

                decimal ratioWhenWindowed = effectiveWindowWidth / (decimal)effectiveWindowHeight;

                if (!((ratioWhenWindowed - ratio16by9) < 0.001m))
                {
                    throw new Exception("Unknown aspect ratio or programmatic error. Try to run 16:9 fullscreen. That usually works. 16:9 windowed should work too.");
                }

                Log.Debug("Detected windowed Fall Guys. Effective width: {0}. Effective height: {1}", effectiveWindowWidth, effectiveWindowHeight);

                // Windowed case: Adds 30px top, and 8 all other sides.
                double relativeStartX = effectiveWindowWidth * xStartPercentage;
                double relativeStartY = effectiveWindowHeight * yStartPercentrage;
                result.Left = (int)relativeStartX + effectiveWindowLeft;
                result.Top = (int)relativeStartY + effectiveWindowTop;

                result.Right = (int)(effectiveWindowWidth * xEndPercentage + effectiveWindowLeft);
                result.Bottom = (int)(effectiveWindowHeight * yEndPercentage + effectiveWindowTop);
            }
            return result;
        }
    }
}