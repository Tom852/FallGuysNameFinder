using Backend.Model;
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
        public History History { get; private set; }
        private ParsingController ParsingController { get; set; }
        private MatchingService ComparisonService { get; set; }
        private StatisticsService StatisticsService { get; set; }

        private ScreenshotService ScreenshotService { get; set; }
        private Options Options { get; set; }

        private bool isInit = false;
        private bool stopRequested = false;
        private bool stopBecauseSuccess = false;

        private Thread t;

        public event EventHandler OnStop;

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
                .WriteTo.File(DataStorageStuff.LogFile, fileSizeLimitBytes: Constants.LogFileSizeLimit, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, shared: true, flushToDiskInterval: TimeSpan.FromSeconds(5), retainedFileCountLimit: Constants.LogFilesToKeep)
                .MinimumLevel.Is(loglevel)
                .CreateLogger();

            Log.Information("Initializing backend engine...");

            History = new History();

            ParsingController = new ParsingController();
            ComparisonService = new MatchingService();
            StatisticsService = new StatisticsService();
            ScreenshotService = new ScreenshotService();
            isInit = true;
            Log.Information("Engine successfully initialized...");

        }

        public void Stop()
        {
            Log.Information("Stopping engine...");
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
                throw new Exception("not initialized");
            }

            Thread.Sleep(Constants.TimeBeforeStart);

            //FgWindowAccess.BringToFront(); // could make an option here to initially foreground the window. but is it necessary?

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
                        iterations--;
                        Thread.Sleep(Constants.TimeWaitWhenFgNotForeground);
                        continue;
                    }


                    PressP();

                    Thread.Sleep(Constants.TimeBetweenPresses + new Random().Next(Constants.TimeVariationBetweenPresses));

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
                        if (match.IsMatching())
                        {
                            switch (match)
                            {
                                case MatchingResult.Alliteration:
                                    Log.Information("Alliteration detected");
                                    break;
                                case MatchingResult.DoubleWord:
                                    Log.Information("Double-word detected");
                                    break;
                                case MatchingResult.Pattern:
                                    Log.Information("Pattern match detected");
                                    break;
                                case MatchingResult.Pool:
                                    Log.Information("Pool match detected");
                                    break;
                                case MatchingResult.NoMatch :
                                    throw new InvalidOperationException("Programmatic error.");
                                default:
                                    Log.Warning("Unknown match type detected.");
                                    break;
                            }

                            PrintFunnySuccessLog();
                            stopBecauseSuccess = true;

                            if (Options.AutoConfirm)
                            {
                                Thread.Sleep(500);
                                PressEscapeAtEnd();
                            }
                            Stop();
                        }
                        else
                        {
                            Log.Information("No match.");
                        }
                    }
                    else
                    {
                        Log.Error("Optical character recognition failed.");
                        ScreenshotService.SaveFullScreenDebugScreenshot("OcrFail");

                        if (this.Options.StopOnError)
                        {
                            Stop();
                        }

                        failsInRow++;
                        if (failsInRow > Constants.FailInRowLimit)
                        {
                            Log.Error($"{Constants.FailInRowLimit} errors occurred in a row. Something seems broken. Option Stop-On-Error overridden. Forcing stop...");
                            ScreenshotService.SaveFullScreenDebugScreenshot("failsInRowLimit");
                            Stop();
                        }
                    }

                    if (!stopRequested)
                    {
                        HandleNameNotChanging();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Unexpected Error");
                    ScreenshotService.SaveFullScreenDebugScreenshot("unexpected_error");
                    if (this.Options.StopOnError)
                    {
                        Stop();
                    }
                }
            }

            StatisticsService.Account(this.History);

            OnStop?.Invoke(this, new EventArgs());
            Log.Information("Engine stopped after {iterations} iterations", iterations);

            if (stopBecauseSuccess)
            {
                PrintDonateMessage();
            }
        }

        private void PrintDonateMessage()
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nIf you like this tool, please consider supporting me at https://www.paypal.me/tomk453 \n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private void PrintFunnySuccessLog()
        {
            string[] successmessages = new string[]
            {
                 "YEEEEEEEETAAASTIC!! We got it!",
                 "Unlike on my Tinder, we got a match here!",
                 "It's a Match!",
                 "Don't forget your matches when you want to make a fire. Speaking of matches, we got one here.",
                 "Tennis match, soccer match, basketball match, name match!",
                 "Forget about Tinder, here is where the real matches happen!",
                 "Holy Bonkus! We got it!"
            };

            var i = new Random().Next(successmessages.Length);
            Log.Information(successmessages[i]);
        }

        private void HandleNameNotChanging()
        {
            if (this.History.WereLastNamesAllEqual(10))
            {
                ScreenshotService.SaveFullScreenDebugScreenshot("namesEqual_10");
                Log.Error("The name does not change anymore. Trying to await server timeouts and trying to remove popup messages did not resolve the issue. Something is broken. You are probably disconnected from the server and can't reroll anymore, until you restart the game. Engine will stop.");
                Stop();
            }
            else if (this.History.WereLastNamesAllEqual(3))
            {
                ScreenshotService.SaveFullScreenDebugScreenshot("namesEqual_preWait");
                Log.Warning("All previous names were equal. Assuming server timeout, connection issue, or Amazon / Support-Id window in front.");
                Log.Information("The engine will now wait 30 seconds to counteract a connection issue.");
                Thread.Sleep(Constants.TimeWaitOnTimeout);
                ScreenshotService.SaveFullScreenDebugScreenshot("namesEqual_postWait");
                Log.Information("The engine will now press Space to remove a possible error message or the Amazon / Support-Id window.");
                PressSpace();
                Thread.Sleep(Constants.TimeWaitAfterSpace);
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
        }

        private static void PressEscapeAtEnd()
        {
            Log.Debug("Pressing ESC");
            SendKeys.SendWait("{ESC}");
        }
    }
}