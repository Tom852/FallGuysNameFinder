using Backend;
using Common.Model;
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
        private ObservableCollection<Pattern> patterns;
        private Options options;
        private bool isRunning;
        private bool isConsoleShown;

        public event PropertyChangedEventHandler PropertyChanged;


        public ObservableCollection<Pattern> Patterns
        {
            get => patterns; set
            {
                patterns = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Patterns)));
            }
        }
        public Options Options
        {
            get => options; set
            {
                options = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Options)));
            }
        }

        public bool IsRunning
        {
            get => isRunning; set
            {
                isRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartStopButtonDesc)));
            }
        }
        public bool IsConsoleShown
        {
            get => isConsoleShown; set
            {
                isConsoleShown = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShowConsoleButtonDesc)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConsoleShown)));
            }
        }

        public string StartStopButtonDesc => IsRunning ? "Stop" : "Start";
        public string ShowConsoleButtonDesc => IsConsoleShown ? "Hide Console" : "Show Console";
        

    }
}
