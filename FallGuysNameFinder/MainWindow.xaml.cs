using Backend;
using Backend.Model;
using Common;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FallGuysNameFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModel ViewModel { get; set; } = new ViewModel();

        private Engine BackendEngine { get; set; }
        private DispatcherTimer PenetrantForeGroundChecker { get; set; }

        private ProbabilityService probabilitySerivce { get; set; }

        private event EventHandler OnSecondaryWindowOpening;

        public MainWindow()
        {
            try
            {

                OnSecondaryWindowOpening += (_1, _2) =>
                {
                    this.ViewModel.Options.AlwaysOnTop = false;
                    DataStorageStuff.SaveOptions(this.ViewModel.Options);
                    this.Topmost = false;
                };

                var options = DataStorageStuff.GetOptions();
                var patterns = DataStorageStuff.ReadPatterns();

                ViewModel.Options = options;
                ViewModel.Patterns = new ObservableCollection<Pattern>(patterns);
                DataContext = ViewModel;

                probabilitySerivce = new ProbabilityService();
                RecalculateProbability();

                InitializeComponent();
                this.Topmost = this.ViewModel.Options.AlwaysOnTop;
                var pool = DataStorageStuff.GetStoredPool();
                this.ViewModel.PoolOptions1 = pool.First.Count();
                this.ViewModel.PoolOptions2 = pool.Second.Count();
                this.ViewModel.PoolOptions3 = pool.Third.Count();


                SetupFallguysProcessChecker();

                InitializeConsole();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void InitializeConsole()
        {
            ConsoleAllocator.ShowConsoleWindow();
            Console.WriteLine("Engine output will go here.");
            Console.WriteLine("Observe it in the beginning to ensure everything works properly.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Tips for better results:");
            Console.WriteLine("- Clean unicolor nameplate background");
            Console.WriteLine("- Small icon in nameplate");
            Console.WriteLine("- Dark nameplate for high contrast");
            Console.WriteLine("- One-worded nickname, that is not similar to a user-name");
            Console.WriteLine("- High resolution");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Recommended Namplate: System Error, Dragonfire, Vaporwave");
            Console.WriteLine("Recommended Nickname: Parkour!, Goalie, Spelunker");
            Console.WriteLine("Recommended Resolution: 1920x1080 or higher");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            Console.WriteLine("You can move this windows to your secondary screens or keep it in background or close it with the 'Hide Console' button.");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("If you X the console, the whole software stops.");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
            ConsoleAllocator.HideConsoleWindow();
        }

        private void SetupFallguysProcessChecker()
        {
            this.PenetrantForeGroundChecker = new DispatcherTimer();
            PenetrantForeGroundChecker.Interval = TimeSpan.FromSeconds(1);
            PenetrantForeGroundChecker.Tick += (a, b) =>
            {
                this.ViewModel.FgStatus = FgWindowAccess.GetFgStatus();
            };
            this.PenetrantForeGroundChecker.Start();
        }

        private void AddPattern_Click(object sender, RoutedEventArgs e)
        {
            OnSecondaryWindowOpening?.Invoke(sender, e);

            var w = new AddPatternWindow();
            w.Show();

            w.OnOkClick += (d1, d2) =>
            {
                AddPattern(w.Vm.Words.ToPattern());
            };
        }

   

        private void EditPattern_Click(object sender, RoutedEventArgs e)
        {

            ShowEditPatternMask(dGrid);
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null)
            {
                ShowEditPatternMask(dataGrid);
            }
        }

        private void RemovePattern_Click(object sender, RoutedEventArgs e)
        {
            var index = dGrid.SelectedIndex;
            if (index != -1)
            {
                RemovePattern(index);
            }
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            DataGrid grid = sender as DataGrid;
            var index = grid.SelectedIndex;

            switch (e.Key)
            {
                case Key.Delete:
                case Key.Back:
                    RemovePattern(index);
                    break;
                case Key.Enter:
                    ShowEditPatternMask(grid);
                    break;
            }
            e.Handled = true;

        }

        private void ShowEditPatternMask(DataGrid dataGrid)
        {
            OnSecondaryWindowOpening?.Invoke(null, null);

            var index = dataGrid.SelectedIndex;
            if (index == -1)
            {
                return;
            }
            var pattern = this.ViewModel.Patterns[index];

            var w = new AddPatternWindow(new StringTriple(pattern));
            w.Show();
            w.OnOkClick += (d1, d2) =>
            {
                EditPattern(index, w.Vm.Words.ToPattern());
            };
        }

        // todo: if more options like this come should make this different. like an evaluate options event stuff.
        private void OnAlwaysOnTopClick(object sender, RoutedEventArgs e)
        {
            DataStorageStuff.SaveOptions(this.ViewModel.Options);
            this.Topmost = this.ViewModel.Options.AlwaysOnTop;
        }

        private void OnOptionsClick(object sender, RoutedEventArgs e)
        {
            DataStorageStuff.SaveOptions(this.ViewModel.Options);
        }

        private void OnOptionsClickWithRecalc(object sender, RoutedEventArgs e)
        {
            DataStorageStuff.SaveOptions(this.ViewModel.Options);
            RecalculateProbability();
        }

        private void AddPattern(Pattern p)
        {
            DataStorageStuff.AddPattern(p);
            this.ViewModel.Patterns.Add(p);
            RecalculateProbability();
        }

        private void RemovePattern(int index)
        {
            DataStorageStuff.RemovePattern(index);
            this.ViewModel.Patterns.RemoveAt(index);

            var totalElements = this.ViewModel.Patterns.Count;
            if (index < totalElements)
            {
                dGrid.SelectedIndex = index;
            }
            else if (index == totalElements)
            {
                dGrid.SelectedIndex = index - 1; //on 0, will be set to -1 which is none selected which is ok.
            }
            RecalculateProbability();
        }

        private void EditPattern(int index, Pattern p)
        {
            DataStorageStuff.EditPattern(index, p);
            this.ViewModel.Patterns.RemoveAt(index);
            this.ViewModel.Patterns.Insert(index, p);
            RecalculateProbability();
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            var dinger = this.probabilitySerivce.AllNamesThatMatch.ToList();
            DataStorageStuff.CreatePreviewList(dinger);
            DataStorageStuff.OpenPreviewList();
        }

            private void ShowConsole_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.IsConsoleShown)
            {
                ConsoleAllocator.HideConsoleWindow();
                ViewModel.IsConsoleShown = false;
            }
            else
            {
                ConsoleAllocator.ShowConsoleWindow();
                ViewModel.IsConsoleShown = true;
            }
        }

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            this.dGrid.SelectedItem = null;
            if (this.ViewModel.EngineStatus != EngineStatus.Stopped)
            {
                this.ViewModel.EngineStatus = EngineStatus.Stopping;
                this.BackendEngine.Stop();
            }
            else
            {
                this.ViewModel.Options = DataStorageStuff.GetOptions();
                this.ViewModel.Patterns = new ObservableCollection<Pattern>(DataStorageStuff.ReadPatterns());

                // hmm, not sure if people want that.
                //if (!this.ViewModel.IsConsoleShown)
                //{
                //    ViewModel.IsConsoleShown = true;
                //    ConsoleAllocator.ShowConsoleWindow();
                //}

                this.BackendEngine = new Engine();
                BackendEngine.Initialize();

                BackendEngine.Start();

                this.BackendEngine.OnStop += (a, b) =>
                {
                    this.ViewModel.EngineStatus = EngineStatus.Stopped;
                };

                this.ViewModel.EngineStatus = EngineStatus.Running;
            }
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            OpenDocPage("userGuide.html");
        }

        private void EditPools_Click(object sender, RoutedEventArgs e)
        {
            OnSecondaryWindowOpening?.Invoke(null, null);

            var w = new PoolWindow();
            w.Show();
            w.OnOkClick += (_, newPool) =>
            {
                RecalculateProbability();
                var pool = DataStorageStuff.GetStoredPool();
                this.ViewModel.PoolOptions1 = newPool.First.Count();
                this.ViewModel.PoolOptions2 = newPool.Second.Count();
                this.ViewModel.PoolOptions3 = newPool.Third.Count();
            };
        }


        



        private void About_Click(object sender, RoutedEventArgs e)
        {
            OpenDocPage("about.html");
        }

        private static void OpenDocPage(string htmlFile)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var dir = System.IO.Path.GetDirectoryName(path);
            var file = System.IO.Path.Combine(dir, "doc", htmlFile);
            System.Diagnostics.Process.Start(file);
        }

        private CancellationTokenSource previousTokenSrc;
        private async void RecalculateProbability()
        {
            this.ViewModel.TimeEstimate = "...";
            this.ViewModel.ChanceToHit = "...";
            this.ViewModel.SelectedCombinations = "...";
            this.ViewModel.ProbabilityIsCalcing = true;

            try
            {
                previousTokenSrc?.Cancel();
                previousTokenSrc?.Dispose();

                previousTokenSrc = new CancellationTokenSource();

                Probability result = await probabilitySerivce.GetProbabilityAsync(new List<Pattern>(ViewModel.Patterns), DataStorageStuff.GetStoredPool(), ViewModel.Options, previousTokenSrc.Token);
                this.ViewModel.TimeEstimate = result.GetTimeRequired();
                this.ViewModel.ChanceToHit = result.GetProbabilityAsFormattedString();
                this.ViewModel.SelectedCombinations = result.GetCombinationsAsFormattedString();

                this.ViewModel.ProbabilityIsCalcing = false;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}