using System;
using System.ComponentModel;

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