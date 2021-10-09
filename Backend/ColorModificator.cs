using System;
using System.Collections.Generic;
using System.Drawing;

namespace Backend
{
    public class ColorModificator
    {

        // patch data okt 21  "paint brightness 0-240"
        // paint ftw

        //normal
        // foregroudn 240
        // plate brightest: 151

        //window in front:
        //text: 206
        //brightest in plkate: 129


        private const int MONOCHROME_BRIGHT_BOUNDARY = 200; // nails it always
        private const int MONOCHROME_WHITE_BOUNDARY = 238;  // specifically for thinning it out
        private const int MONOCHROME_WHEN_IN_BG_BOUNDARY = 160;  // specifically for error msg.

        public List<Bitmap> GetAll(Bitmap bmp)
        {
            List<Bitmap> result = new List<Bitmap>
            {
                // todo: if the nameplate is definitely fixed to the blue one, flexibilty is not really necessary, grayscale pretty useless etc. first 3 would be fine.

                ToMonochrome(bmp, MONOCHROME_BRIGHT_BOUNDARY, true),
                ToMonochrome(bmp, MONOCHROME_WHITE_BOUNDARY, true),
                ToMonochrome(bmp, MONOCHROME_WHEN_IN_BG_BOUNDARY, true),

                ToGrayScale(bmp, true),

                new Bitmap(bmp),
                Invert(bmp),

                // I am not sure if the non-inverted images make sense. It seemed like if the inverted fails, this one does too...
                ToGrayScale(bmp, false),
                ToMonochrome(bmp, MONOCHROME_BRIGHT_BOUNDARY, false),
                ToMonochrome(bmp, MONOCHROME_WHITE_BOUNDARY, false),
                ToMonochrome(bmp, MONOCHROME_WHEN_IN_BG_BOUNDARY, false)
            };

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
                    brightness = GetPaintBrightness(c);
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
                    brightness = GetPaintBrightness(c);

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

        private int GetPaintBrightness(Color c)
        {
            var floaty = c.GetBrightness();
            return (int)(240*floaty);
        }
    }
}