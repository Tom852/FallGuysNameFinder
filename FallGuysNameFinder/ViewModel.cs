using Common;
using Common.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace FallGuysNameFinder
{
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Pattern> patterns;
        private Options options;
        private bool isConsoleShown;
        private FgStatus fgStatus = FgStatus.NotRunning;
        private EngineStatus engineStatus = EngineStatus.Stopped;
        private string chanceToHit;
        private string selectedCombinations;
        private string timeEstimate;
        private bool probabilityIsCalcing;
        private int poolOptionsActiveAsNumber;

        public event PropertyChangedEventHandler PropertyChanged;

        public string SelectedCombinations
        {
            get => selectedCombinations;
            set
            {
                selectedCombinations = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCombinations)));
            }
        }


        public int PoolOptionsActiveAsNumber
        {
            get => poolOptionsActiveAsNumber; set
            {
                poolOptionsActiveAsNumber = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PoolOptionsActive)));
            }
        }

        public string PoolOptionsActive => $"{poolOptionsActiveAsNumber.ToString("n0")} pool combinations selected";

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

        public FgStatus FgStatus
        {
            get => fgStatus;
            set
            {
                if (value == fgStatus) return;
                fgStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FgStatus)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FgStatusIcon)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FgStatusDescription)));
            }
        }

        // todo: mal noch aufräumen hier... ist mehr drin als nur vm.
        public Icon FgStatusIcon
        {
            get
            {
                switch (fgStatus)
                {
                    case FgStatus.Foreground:
                        return SystemIcons.Information;

                    case FgStatus.RunningButNoFocus:
                        return SystemIcons.Warning;

                    case FgStatus.NotRunning:
                        return SystemIcons.Error;

                    default:
                        throw new Exception("Unknown Fall Guys Status");
                }
            }
        }

        public string FgStatusDescription
        {
            get
            {
                switch (fgStatus)
                {
                    case FgStatus.Foreground:
                        return "Foreground";

                    case FgStatus.RunningButNoFocus:
                        return "Background";

                    case FgStatus.NotRunning:
                        return "Not Found";

                    default:
                        throw new Exception("Unknown Fall Guys Status");
                }
            }
        }
        public bool ProbabilityIsCalcing // todo: could use this for the ... display as well at the percentage and time estimate
        {
            get => probabilityIsCalcing; set
            {
                probabilityIsCalcing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProbabilityIsCalcing)));
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

        public bool IsRunning => engineStatus == EngineStatus.Running;

        public EngineStatus EngineStatus
        {
            get => engineStatus; set
            {
                engineStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EngineStatus)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EngineStatusDescription)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EngineStatusIcon)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartStopButtonDesc)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
            }
        }

        public Icon EngineStatusIcon
        {
            get
            {
                switch (engineStatus)
                {
                    case EngineStatus.Running:
                        return SystemIcons.Information;

                    case EngineStatus.Stopping:
                        return SystemIcons.Warning;

                    case EngineStatus.Stopped:
                        return SystemIcons.Error;

                    default:
                        throw new Exception("Unknown Engine Status");
                }
            }
        }

        public string StartStopButtonDesc
        {
            get
            {
                switch (engineStatus)
                {
                    case EngineStatus.Running:
                        return "Stop";

                    case EngineStatus.Stopping:
                        return "Stopping...";

                    case EngineStatus.Stopped:
                        return "Start";

                    default:
                        throw new Exception("Unknown Engine Status");
                }
            }
        }

        public string EngineStatusDescription
        {
            get => engineStatus.ToString();
        }

        public string ShowConsoleButtonDesc => IsConsoleShown ? "Hide Console" : "Show Console";

        public string ChanceToHit
        {
            get => chanceToHit;
            set
            {
                chanceToHit = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChanceToHit)));
            }
        }

        public string TimeEstimate
        {
            get => timeEstimate;
            set
            {
                timeEstimate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TimeEstimate)));
            }
        }
    }
}