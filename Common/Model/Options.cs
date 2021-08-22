using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Options : INotifyPropertyChanged
    {
        private bool stopOnAlliteration;
        private bool stopOnDoubleWord;
        private bool stopOnError;

        public bool StopOnAlliteration
        {
            get => stopOnAlliteration;
            set
            {
                stopOnAlliteration = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopOnAlliteration)));
            }
        }
        public bool StopOnDoubleWord
        {
            get => stopOnDoubleWord;
            set
            {
                stopOnDoubleWord = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopOnDoubleWord)));
            }
        }
        public bool StopOnError
        {
            get => stopOnError;
            set
            {
                stopOnError = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopOnError)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
