using Backend;
using Common.Model;
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
using System.Windows.Shapes;

namespace FallGuysNameFinder
{
    /// <summary>
    /// Interaction logic for AddPAtternWindow.xaml
    /// </summary>
    public partial class AddPatternWindow : Window
    {
        public event EventHandler OnOkClick;
        public event EventHandler OnCancelClick;
        public event EventHandler OnRemoveClick;

        public AddPatternViewModel Vm { get; set; }

        public AddPatternWindow(Pattern p)
        {

            var vm = new AddPatternViewModel();
            vm.Pattern = p;
            vm.FirstNames = PossibleNames.FirstNames().ToList().OrderBy(s => s).ToList();
            vm.SecondNames = PossibleNames.SecondNames().ToList().OrderBy(s => s).ToList();
            vm.ThirdNames = PossibleNames.ThirdNames().ToList().OrderBy(s => s).ToList();
            vm.FirstNames.Insert(0, "*");
            vm.SecondNames.Insert(0, "*");
            vm.ThirdNames.Insert(0, "*");

            this.Vm = vm;
            this.DataContext = Vm;
            InitializeComponent();

        }

        public AddPatternWindow()
            : this(Pattern.GetEmpty())
        {
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var isValid1 = this.Vm.FirstNames.Contains(this.Vm.Pattern.First);
            var isValid2 = this.Vm.SecondNames.Contains(this.Vm.Pattern.Second);
            var isValid3 = this.Vm.ThirdNames.Contains(this.Vm.Pattern.Third);

            if (!isValid1 || !isValid2 || !isValid3)
            {
                MessageBox.Show("At least one name is not valid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                OnOkClick?.Invoke(this, e);
                this.Close();
            }


        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            OnCancelClick?.Invoke(this, e);
            this.Close();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            OnRemoveClick?.Invoke(this, e);
            this.Close();
        }
    }
}
