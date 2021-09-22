using Common;
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
    /// Interaction logic for PoolWindow.xaml
    /// </summary>
    public partial class PoolWindow : Window
    {
        public event EventHandler<Pool> OnOkClick;

        public PoolWindow()
        {
            this.DataContext = new PoolViewModel();
            InitializeComponent();
            var pool = DataStorageStuff.GetStoredPool();
            foreach (var item in pool.First)
            {
                List1.SelectedItems.Add(item);
            }
            foreach (var item in pool.Second)
            {
                List2.SelectedItems.Add(item);
            }
            foreach (var item in pool.Third)
            {
                List3.SelectedItems.Add(item);
            }

        }


        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            var newPool = new Pool();

            foreach (var i in List1.SelectedItems)
            {
                newPool.First.Add((string)i);
            }
            foreach (var i in List2.SelectedItems)
            {
                newPool.Second.Add((string)i);
            }
            foreach (var i in List3.SelectedItems)
            {
                newPool.Third.Add((string)i);
            }
            DataStorageStuff.SavePool(newPool);

            OnOkClick?.Invoke(this, newPool);

            this.Close();
        }
    }
}
