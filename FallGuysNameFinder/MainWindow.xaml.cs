using Backend;
using Common;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
        public Engine BackendEngine { get; private set; }

        public MainWindow()
        {
            DataStuff = new DataStorageStuff();
            var options = DataStuff.GetOptions();
            var patterns = DataStuff.ReadPatterns();

            ViewModel.Options = options;
            ViewModel.Patterns = new ObservableCollection<Pattern>(patterns);
            DataContext = ViewModel;

            InitializeComponent();
        }

        private void AddPattern_Click(object sender, RoutedEventArgs e)
        {
            var w = new AddPatternWindow();
            w.Show();
            w.OnOkClick += (d1, d2) =>
            {
                AddThePattern(w.Pattern);
            };
        }



        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid != null)
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
                    EditPattern(index, w.Pattern);
                };
                w.OnRemoveClick += (d1, d2) =>
                {
                    RemovePattern(index);
                };

            }
        }

        private void OnOptionsClick(object sender, RoutedEventArgs e)
        {
            this.DataStuff.SaveOptions(this.ViewModel.Options);
        }

        private void AddThePattern(Pattern p)
        {
            DataStuff.AddPattern(p);
            this.ViewModel.Patterns.Add(p);
        }

        private void RemovePattern(int index)
        {
            DataStuff.RemovePattern(index);
            this.ViewModel.Patterns.RemoveAt(index);
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
            if (this.ViewModel.IsRunning)
            {
                this.BackendEngine.Stop();
            }
            else
            {
                this.ViewModel.Options = DataStuff.GetOptions();
                this.ViewModel.Patterns = new ObservableCollection<Pattern>(DataStuff.ReadPatterns());

                if (!this.ViewModel.IsConsoleShown)
                {
                    ViewModel.IsConsoleShown = true;
                    ConsoleAllocator.ShowConsoleWindow();
                }

                this.BackendEngine = new Engine();
                BackendEngine.Initialize();

 
                
                BackendEngine.Start();
                this.ViewModel.IsRunning = true;
                this.BackendEngine.OnStop += (a, b) =>
                {
                    this.ViewModel.IsRunning = false;
                };
            }

        }
    }
}
