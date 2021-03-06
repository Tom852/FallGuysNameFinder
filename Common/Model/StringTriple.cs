using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Common.Model
{
    public class StringTriple : INotifyPropertyChanged
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

        public StringTriple() { }

        public StringTriple(string first, string second, string third) => (First, Second, Third) = (first, second, third);

        public StringTriple(IEnumerable<string> words)
            : this(words.ToArray())
        {
        }

        public StringTriple(string[] words)
        {
            if (words.Length != 3)
            {
                throw new ArgumentException("Cannot construct name out of != 3 words");
            }
            First = words[0];
            Second = words[1];
            Third = words[2];
        }

        public StringTriple(Pattern p) : this(new string[] { p.First, p.Second, p.Third })
        {
        }

        public Name ToName()
        {
            return new Name(First, Second, Third);
        }

        public Pattern ToPattern()
        {
            return new Pattern(First, Second, Third);
        }

        public override string ToString()
        {
            return $"{First} {Second} {Third}";
        }

        public static bool operator ==(StringTriple left, StringTriple right) => left.Equals(right);

        public static bool operator !=(StringTriple left, StringTriple right) => !left.Equals(right);

        public override bool Equals(object obj)
        {
            return obj is StringTriple name &&
                   First == name.First &&
                   Second == name.Second &&
                   Third == name.Third;
        }

        public override int GetHashCode()
        {
            int hashCode = 2144946132;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(First);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Second);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Third);
            return hashCode;
        }
    }
}