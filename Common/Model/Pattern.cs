using System;
using System.ComponentModel;
using System.Linq;

namespace Common.Model
{
    public class Pattern : INotifyPropertyChanged
    {
        private string first;
        private string second;
        private string third;

        public event PropertyChangedEventHandler PropertyChanged;

        public string First
        {
            get => first;
            set
            {
                first = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(First)));
            }
        }

        public string Second
        {
            get => second;
            set
            {
                second = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Second)));
            }
        }

        public string Third
        {
            get => third;
            set
            {
                third = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Third)));
            }
        }

        public const string Wildcard = "*";

        public Pattern(string s)
            : this(s.Split(' '))
        {
        }

        public Pattern(string[] splitted)
        {
            if (splitted.Length != 3)
            {
                throw new Exception("Pattern must contain of 3 words separated by space.");
            }

            var firstgood = splitted[0] == Wildcard || PossibleNames.FirstNames(false).Contains(splitted[0]);
            var secondgood = splitted[1] == Wildcard || PossibleNames.SecondNames(false).Contains(splitted[1]);
            var thirdgood = splitted[2] == Wildcard || PossibleNames.ThirdNames(false).Contains(splitted[2]);

            if (!firstgood || !secondgood || ! thirdgood)
            {
                throw new ArgumentException($"{splitted[0]} {splitted[1]} {splitted[2]} is not a viable Pattern");
            }
            First = splitted[0];
            Second = splitted[1];
            Third = splitted[2];
        }

        public static Pattern GetEmpty()
        {
            return new Pattern(new string[] { string.Empty, string.Empty, string.Empty });
        }

        public Pattern Clone()
        {
            var s = new string[] { First, Second, Third };
            return new Pattern(s);
        }

        public override string ToString()
        {
            return $"{First} {Second} {Third}";
        }
    }
}