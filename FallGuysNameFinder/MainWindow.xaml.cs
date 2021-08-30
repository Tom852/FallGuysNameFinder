using Backend;
using Common;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FallGuysNameFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModel ViewModel { get; set; } = new ViewModel();
        public DataStorageStuff DataStuff { get; }
        public Engine BackendEngine { get; set; }
        public DispatcherTimer PenetrantForeGroundChecker { get; set; }

        public MainWindow()
        {
            DataStuff = new DataStorageStuff();
            var options = DataStuff.GetOptions();
            var patterns = DataStuff.ReadPatterns();

            ViewModel.Options = options;
            ViewModel.Patterns = new ObservableCollection<Pattern>(patterns);
            DataContext = ViewModel;

            InitializeComponent();

            SetupFallguysProcessChecker();

            InitializeConsole();
        }

        private void InitializeConsole()
        {
            ConsoleAllocator.ShowConsoleWindow();
            Console.WriteLine("Engine output will go here.");
            Console.WriteLine("Observe it in the beginning to ensure everything works properly.");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Tips for bettter results:");
            Console.WriteLine("- Clean unicolor nameplate background");
            Console.WriteLine("- Small icon in nameplate");
            Console.WriteLine("- Dark namepalte for high contrast");
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
            var w = new AddPatternWindow();
            w.Show();
            w.OnOkClick += (d1, d2) =>
            {
                AddPattern(w.Vm.Pattern);
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
            var index = dGrid.SelectedIndex;

            switch (e.Key)
            {
                case Key.Delete:
                case Key.Back:
                    RemovePattern(index);
                    e.Handled = true;
                    break;
            }
        }

        private void ShowEditPatternMask(DataGrid dataGrid)
        {
            var index = dataGrid.SelectedIndex;
            if (index == -1)
            {
                return;
            }
            var pattern = this.ViewModel.Patterns[index];
            var clone = pattern.Clone();
            var w = new AddPatternWindow(clone);
            w.Show();
            w.OnOkClick += (d1, d2) =>
            {
                EditPattern(index, w.Vm.Pattern);
            };
            w.OnRemoveClick += (d1, d2) =>
            {
                RemovePattern(index);
            };
        }

        private void OnOptionsClick(object sender, RoutedEventArgs e)
        {
            this.DataStuff.SaveOptions(this.ViewModel.Options);
        }

        private void AddPattern(Pattern p)
        {
            DataStuff.AddPattern(p);
            this.ViewModel.Patterns.Add(p);
        }

        private void RemovePattern(int index)
        {
            DataStuff.RemovePattern(index);
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
        }

        private void EditPattern(int index, Pattern p)
        {
            DataStuff.EditPattern(index, p);
            this.ViewModel.Patterns.RemoveAt(index);
            this.ViewModel.Patterns.Insert(index, p);
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
            if (this.ViewModel.EngineStatus != EngineStatus.Stopped)
            {
                this.ViewModel.EngineStatus = EngineStatus.Stopping;
                this.BackendEngine.Stop();
            }
            else
            {
                this.ViewModel.Options = DataStuff.GetOptions();
                this.ViewModel.Patterns = new ObservableCollection<Pattern>(DataStuff.ReadPatterns());

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
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var dir = System.IO.Path.GetDirectoryName(path);
            var file = System.IO.Path.Combine(dir, "doc", "userGuide.html");
            System.Diagnostics.Process.Start(file);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var dir = System.IO.Path.GetDirectoryName(path);
            var file = System.IO.Path.Combine(dir, "doc", "about.txt");
            var txt = System.IO.File.ReadAllText(file);

            var window = new About();
            window.Text.Text = txt;
            window.Show();
        }
    }
}
