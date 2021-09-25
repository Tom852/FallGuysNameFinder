using Backend.Model;
using Common;
using Common.Model;
using System;
using System.Linq;
using System.Windows;

namespace FallGuysNameFinder
{
    /// <summary>
    /// Interaction logic for AddPAtternWindow.xaml
    /// </summary>
    public partial class AddPatternWindow : Window
    {
        public event EventHandler OnOkClick;

        public event EventHandler OnCancelClick;

        public AddPatternViewModel Vm { get; set; }

        public AddPatternWindow(StringTriple words)
        {
            var vm = new AddPatternViewModel();
            vm.Words = words;
            vm.FirstNames = PossibleNames.FirstNames(false).ToList().OrderBy(s => s).ToList();
            vm.SecondNames = PossibleNames.SecondNames(false).ToList().OrderBy(s => s).ToList();
            vm.ThirdNames = PossibleNames.ThirdNames(false).ToList().OrderBy(s => s).ToList();
            vm.FirstNames.Insert(0, "*");
            vm.SecondNames.Insert(0, "*");
            vm.ThirdNames.Insert(0, "*");

            this.Vm = vm;
            this.DataContext = Vm;

            InitializeComponent();
        }

        public AddPatternWindow()
            : this(new StringTriple())
        {
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var isValid1 = this.Vm.FirstNames.Contains(this.Vm.Words.First);
            var isValid2 = this.Vm.SecondNames.Contains(this.Vm.Words.Second);
            var isValid3 = this.Vm.ThirdNames.Contains(this.Vm.Words.Third);

            if (this.Vm.Words.Third == "Grabber")
            {
                MessageBox.Show("You can't select Grabber as the third name because I hate grabbers!!", "Nope", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (!isValid1 || !isValid2 || !isValid3)
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
    }
}