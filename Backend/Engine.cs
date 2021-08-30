using Common;
using Common.Model;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Backend
{
    public class Engine
    {
        private const int FailInRowLimit = 10;

        public History History { get; private set; }
        private ParsingController ParsingController { get; set; }
        private MatchingService ComparisonService { get; set; }
        private Options Options { get; set; }

        private bool isInit = false;
        private bool stopRequested = false;

        private Thread t;

        public event EventHandler OnStop;

        public static void Main()
        {
        } // todo: transofmr to lib.

        public void Initialize()
        {
            Options = DataStorageStuff.GetOptions();

            LogEventLevel loglevel;

            if (Options.Verbose)
            {
                loglevel = Serilog.Events.LogEventLevel.Debug;
            }
            else
            {
                loglevel = Serilog.Events.LogEventLevel.Information;
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(DataStorageStuff.LogFile, fileSizeLimitBytes: 10 * 1024 * 1024, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(5), retainedFileCountLimit: 3)
                .MinimumLevel.Is(loglevel)
                .CreateLogger();

            Log.Information("Initializing Backend Engine...");

            History = new History();

            ParsingController = new ParsingController();
            ComparisonService = new MatchingService();
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

                    bool success = ParsingController.ReadFromScreen();
                    if (success)
                    {
                        var name = ParsingController.Result;
                        failsInRow = 0;
                        this.History.Add(name);

                        var match = ComparisonService.Test(name);
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
                            Log.Fatal($"{FailInRowLimit} fails in a row. Something is broken. Option Stop-On-Error overridden. Forcing stop...");
                            Stop();
                        }
                    }

                    HandleServerErrorMessage();
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

            new StatisticsService().Account(this.History);

            OnStop?.Invoke(this, new EventArgs());
            Log.Information("Engine stopped after {iterations} iterations", iterations);
        }

        private void HandleServerErrorMessage()
        {
            if (this.History.WereLastNamesAllEqual(8))
            {
                Log.Error("Still no improvement. Something is broken. Engine will Stop.");
                Stop();
            }
            else if (this.History.WereLastNamesAllEqual(6))
            {
                Log.Warning("Yet no improvement. Pressing SPACE again for the case we lifted the Amazon Link Page or revealed support id...");
                PressSpace();
            }
            else if (this.History.WereLastNamesAllEqual(5))
            {
                Log.Warning("All 5 previous names were equal. Assuming a window in front. The engine will now press SPACE to get rid of a possible overlay message.");
                PressSpace();
            }
        }

        private static void PressP()
        {
            Log.Debug("Pressing P");
            SendKeys.SendWait("{P}");
        }

        private static void PressSpace()
        {
            Log.Debug("Pressing SPACE");
            SendKeys.SendWait(" ");
            Thread.Sleep(500);
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

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