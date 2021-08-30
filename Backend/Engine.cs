using Common;
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

        const int FailInRowLimit = 10;

        private OcrService OcrService { get; set; }
        private WordsComparisonService ComparisonService { get; set; }
        private Options Options { get; set; }

        private bool isInit = false;
        private bool stopRequested = false;

        private Thread t;

        public event EventHandler OnStop;

        public static void Main() { } // todo: transofmr to lib.

        public void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(DataStorageStuff.LogNamesFile, fileSizeLimitBytes: 10 * 1024 * 1024, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(5), retainedFileCountLimit: 3)
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
                .CreateLogger();


            Log.Information("Initializing Backend Engine...");

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

            Thread.Sleep(3000);


            //BringToFront(); // could make an option here but propbably not necessary?

            int iterations = 0;
            int failsInRow = 0;
            while (!stopRequested)
            {
                iterations++;
                try
                {
                    if (!FgWindowAccess.IsFgInForeGround())
                    {
                        Log.Information("Fall Guys is not in foreground. Iteration skipped.");
                        Thread.Sleep(4000);
                        continue;
                    }

                    PressP();

                    Thread.Sleep(3750 + new Random().Next(500));

                    if (stopRequested)
                    {
                        break;
                    }

                    bool success = OcrService.ReadFromScreen(out var text);
                    if (success)
                    {
                        failsInRow = 0;

                        var match = ComparisonService.Test(text);
                        if (match)
                        {
                            Log.Information("YEEEEEEEETAAASTIC!! WE GOT IT!!!");
                            Stop();
                        }
                        else
                        {
                            Log.Debug("Pattern did not match, rerolling...");
                        }
                    }
                    else
                    {

                        Log.Error("Optical Character Recognition failed.");
                        if (this.Options.StopOnError)
                        {
                            Log.Information("Stopping Engine.");
                            Stop();
                        }

                        failsInRow++;
                        if (failsInRow > FailInRowLimit)
                        {
                            Log.Fatal("10 fails in a row. Something is broken. Option Stop-On-Error overridden. Forcing stop...");
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
            Log.Information("Engine stopped after {iterations} iterations", iterations);
        }


        private static void PressP()
        {
                    Log.Debug("Pressing P");
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
                Log.Warning("Fall Guys is not running and cannot be brought to Foreground.");
            }

        }
    }
}
