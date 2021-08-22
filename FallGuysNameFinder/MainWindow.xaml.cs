using Backend;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace FallGuysNameFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModel ViewModel { get; set; } = new ViewModel();
        public MainWindow()
        {
            var dataStuff = new DataStorageStuff();
            var options = dataStuff.GetOptions();
            var patterns = dataStuff.ReadPatterns();

            ViewModel.StopOnAlliteration = options.StopOnAlliteration;
            ViewModel.StopOnDoubleWord = options.StopOnDoubleWord;
            ViewModel.Patterns = new ObservableCollection<Pattern>(patterns);
            DataContext = ViewModel;

            InitializeComponent();
        }

        private void AddPattern_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
