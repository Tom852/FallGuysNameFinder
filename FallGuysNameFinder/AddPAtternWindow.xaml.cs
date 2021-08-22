using Backend;
using Common.Model;
using System;
using System.Collections.Generic;
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

        public Pattern Pattern { get; set; }

        public AddPatternWindow(Pattern p)
        {
            this.Pattern = p;
            this.DataContext = Pattern;
            InitializeComponent();
        }

        public AddPatternWindow()
            : this(Pattern.GetEmpty())
        {
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            OnOkClick?.Invoke(this, e);
            this.Close();
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
