using System.ComponentModel;

namespace Common.Model
{
    public class Options : INotifyPropertyChanged
    {
        private bool stopOnAlliteration;
        private bool stopOnDoubleWord;
        private bool stopOnError;
        private bool alwaysOnTop;
        private bool verbose = true;
        private bool autoConfirm = true;

        public bool AlwaysOnTop
        {
            get => alwaysOnTop;
            set
            {
                alwaysOnTop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AlwaysOnTop)));
            }
        }

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

        public bool AutoConfirm
        {
            get => autoConfirm;
            set
            {
                autoConfirm = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoConfirm)));
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

        public bool Verbose
        {
            get => verbose;
            set
            {
                verbose = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Verbose)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}