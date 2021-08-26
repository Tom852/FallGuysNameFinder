using Common.Model;
using Serilog;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Backend
{
    public class Engine
    {
        private OcrService OcrService { get; set; }
        private WordsComparisonService ComparisonService { get; set; }
        private Options Options { get; set; }

        private bool isInit = false;
        private bool stopRequested = false;

        private Thread t;

        public event EventHandler OnStop;

        public static void Main() { } // todo:
        public void Initialize()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug).CreateLogger();


            Console.WriteLine("Initializing Backend Engine...");
            Console.WriteLine("Note: Use a simple background with high contrast and no diggly-thingie butterflies on it.");
            Console.WriteLine("Note: If the results are garbage, try another background.");

            var dataStorageStuff = new DataStorageStuff();


            OcrService = new OcrService();
            ComparisonService = new WordsComparisonService(dataStorageStuff);
            this.Options = dataStorageStuff.GetOptions();
            isInit = true;
        }

        public void Stop()
        {
            stopRequested = true;
        }

        public void Start()
        {
            t = new Thread(() => Run());
            t.Start();
        }

        private void Run()
        {
            if (!isInit)
            {
                throw new Exception("not initialized!");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Ensure your Fall Guys is running, the profil page is open and it is in keyboard-focus.");
            Console.WriteLine("You can move these windows to your secondary screens or keep them in background.");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine("Starting Backend Engine in");

            for (int i = 5; i>=1; i--)
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);
            }
            Console.WriteLine();


            //BringToFront();

            int iterations = 0;
            while (!stopRequested)
            {
                iterations++;
                try
                {
                    //BringToFront();
                    //Thread.Sleep(500);
                    //Its too aggressive.

                    Log.Debug("Pressing P");
                    PressP();

                    Thread.Sleep(4000);
                    Log.Debug("Running OCR...");
                    bool success = OcrService.ReadFromScreen(out var text);
                    if (success)
                    {
                        var match = ComparisonService.Test(text);
                        if (match)
                        {
                            Log.Information("YEEEEEEEETAAASTIC!! WE GOT IT!!!");
                            Stop();
                        }
                        else
                        {
                            Log.Information("Pattern did not match, rerolling...");
                        }
                    }
                    else
                    {
                        Log.Warning("Optical Character Recognition failed.");
                        if (this.Options.StopOnError)
                        {
                            Log.Information("Stopping Engine.");
                            Stop();
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.Error(e, "Unexpected Error");
                    if (this.Options.StopOnError)
                    {
                        Log.Information("Stopping Engine.");
                        Stop();
                    }
                }
            }

            OnStop?.Invoke(this, new EventArgs());
            Log.Information("Engine Stop after {iterations} iterations", iterations);
        }


        private static void PressP()
        {
            SendKeys.SendWait("{P}");
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);



        private void BringToFront()
        {
            try
            {
                var p = Process.GetProcessesByName("FallGuys_client_game");
                SetForegroundWindow(p[0].MainWindowHandle);
            }
            catch
            {
                Log.Warning("Didnt find Fall Guy Window");
            }

        }
    }
}
