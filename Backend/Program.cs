using Serilog;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Backend
{
    public class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            var sss = new ScreenshotService();
            var ocrService = new OcrService();
            var comparisonService = new WordsComparisonService(new DataStorageStuff());

            Console.WriteLine("Press Enter to Start.");
            Console.WriteLine("The sure the Name generating Page is open and your name is visible.");
            Console.WriteLine("Use a simple background with high contrast and no diggly-thingie butterflies on it.");
            Console.WriteLine("If the results are garbage, try another background.");
            Console.WriteLine("Press Enter To Start...");
            Console.ReadKey();
            Console.WriteLine("Starting Engine in 5 seconds....\nMake Sure FG is in Keyboard-Focus!");
            Thread.Sleep(5000);


            while (true)
            {
                try
                {
                    Log.Debug("Keypress executed...");
                    PressT();

                    Thread.Sleep(4000);
                    Log.Debug("Running OCR...");
                    var bmp = sss.TakeScerenshot();
                    var text = ocrService.DoOcr(bmp);
                    var match = comparisonService.Test(text);
                    if (match)
                    {
                        Log.Information("SUCCU SACCA OMG WE GOT IT!!!");
                        break;
                    }
                    else
                    {
                        Log.Information("Pattern did not match, rerolling...");

                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Unexcpetd Error - continuing...");
                }
            }

        }


        private static void PressT()
        {
            SendKeys.SendWait("{P}");
        }
    }
}
