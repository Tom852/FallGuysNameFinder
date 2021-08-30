﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class ColorModificator
    {
        const int MONOCHROME_WHITE_BOUNDARY = 250;
        const int MONOCHROME_BRIGHT_BOUNDARY = 200;
        const int MONOCHROME_WHEN_IN_BG_BOUNDARY = 150;


        public List<Bitmap> GetAll(Bitmap bmp)
        {

            List<Bitmap> result = new List<Bitmap>();

            result.Add(ToMonochrome(bmp, MONOCHROME_WHITE_BOUNDARY, true));
            result.Add(ToMonochrome(bmp, MONOCHROME_BRIGHT_BOUNDARY, true));
            result.Add(ToMonochrome(bmp, MONOCHROME_WHEN_IN_BG_BOUNDARY, true));

            result.Add(ToGrayScale(bmp, false));

            result.Add(new Bitmap(bmp));
            result.Add(Invert(bmp));

            // I am not sure if the non-inverted images make sense. It seemed like if the inverted fails, this one does too...
            result.Add(ToGrayScale(bmp, false));
            result.Add(ToMonochrome(bmp, MONOCHROME_WHITE_BOUNDARY, false));
            result.Add(ToMonochrome(bmp, MONOCHROME_BRIGHT_BOUNDARY, false));
            result.Add(ToMonochrome(bmp, MONOCHROME_WHEN_IN_BG_BOUNDARY, false));

            return result;
        }

        public Bitmap ToMonochrome(Bitmap bmp, int whiteBoundary, bool inverted)
        {
            int brightness;
            Color c;
            var result = new Bitmap(bmp);

            for (int y = 0; y < result.Height; y++)
                for (int x = 0; x < result.Width; x++)
                {
                    c = result.GetPixel(x, y);
                    brightness = GetAverageBrightness(c);
                    if (brightness > whiteBoundary)
                    {
                        if (inverted)
                        {
                            result.SetPixel(x, y, Color.Black);
                        }
                        else
                        {
                            result.SetPixel(x, y, Color.White);
                        }
                    }
                    else
                    {
                        if (inverted)
                        {
                            result.SetPixel(x, y, Color.White);
                        }
                        else
                        {
                            result.SetPixel(x, y, Color.Black);
                        }
                    }
                }
            return result;
        }

        public Bitmap ToGrayScale(Bitmap bmp, bool inverted)
        {
            var result = new Bitmap(bmp);

            int brightness;
            Color c;

            for (int y = 0; y < result.Height; y++)
                for (int x = 0; x < result.Width; x++)
                {
                    c = result.GetPixel(x, y);
                    brightness = GetAverageBrightness(c);
                    if (inverted)
                    {
                        brightness = 255 - brightness;
                    }
                    result.SetPixel(x, y, Color.FromArgb(brightness, brightness, brightness));
                }
            return result;
        }

        public Bitmap Invert(Bitmap bmp)
        {
            var result = new Bitmap(bmp);

            Color c;

            for (int y = 0; y < result.Height; y++)
                for (int x = 0; x < result.Width; x++)
                {
                    c = result.GetPixel(x, y);
                    c = Color.FromArgb(255, (255 - c.R), (255 - c.G), (255 - c.B));
                    result.SetPixel(x, y, c);
                }
            return result;
        }

        private int GetAverageBrightness(Color c) => (c.B + c.G + c.B) / 3;

       // (int) Math.Round(.299 * c.R + .587 * c.G + .114 * c.B);
    }
}
