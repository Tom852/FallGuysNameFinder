using Serilog;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Backend
{
    public class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug).CreateLogger();
            var ocrService = new OcrService();
            var comparisonService = new WordsComparisonService(new DataStorageStuff());

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
                    Log.Debug("Pressing P");
                    PressP();

                    Thread.Sleep(4000);
                    Log.Debug("Running OCR...");
                    bool success = ocrService.ReadFromScreen(out var text);
                    if (success)
                    {
                        var match = comparisonService.Test(text);
                        if (match)
                        {
                            Log.Information("YEEEEEEEETAAASTIC!! OMG WE GOT IT!!!");
                            Console.ReadKey();
                            break;
                        }
                        else
                        {
                            Log.Information("Pattern did not match, rerolling...");
                        }
                    } else
                    {
                        Log.Warning("Could not parse text. Please confirm manually to continue by pressing enter.");
                        Console.ReadKey();
                    }
                    
                }
                catch (Exception e)
                {
                    Log.Error(e, "Unexcpetd Error - continuing...");
                }
            }

        }


        private static void PressP()
        {
            SendKeys.SendWait("{P}");
        }
    }
}
