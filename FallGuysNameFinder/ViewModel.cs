using Backend;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallGuysNameFinder
{
    public class ViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Pattern> Patterns { get; set; }


        private bool _a;
        public bool StopOnAlliteration
        {
            get => _a;
            set
            {
                if (_a != value)
                {
                    _a = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopOnAlliteration)));
                }
            }
        }

        private bool _dw;
        public bool StopOnDoubleWord
        {
            get => _dw;
            set
            {
                if (_dw != value)
                {
                    _dw = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopOnDoubleWord)));
                }
            }
        }
    }
}
