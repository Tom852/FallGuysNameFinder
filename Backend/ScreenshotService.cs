using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backend
{
    public class ScreenshotService
    {

        public Bitmap TakeScerenshot()
        {

            var screenSize = Screen.PrimaryScreen.Bounds.Size;

            if (screenSize.Width != 1920)
            {
                throw new Exception("Only 1920x1080 and 1920x1200 supported.");
            }

            int startX = 1160;
            int startY = 320;

            if (screenSize.Height == 1200)
            {
                startY += 60;
            }

            int widthX = 270;
            int widthY = 35;

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

            bmpScreenshot.Save(@"C:\Test\Screenshot.png", ImageFormat.Png);
            return bmpScreenshot;
        }

    }
}
