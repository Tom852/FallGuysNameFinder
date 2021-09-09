using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Model
{
    public struct Name
    {
        public string First { get; set; }
        public string Second { get; set; }
        public string Third { get; set; }

        public Name(string first, string second, string third)
            : this(new string[] { first, second ,third})
        {
        }

        public Name(string[] words)
        {
            if (words.Length != 3)
            {
                throw new ArgumentException("Cannot construct name out of != 3 words");
            }

            var firstgood = PossibleNames.FirstNames(false).Contains(words[0]);
            var secondgood = PossibleNames.SecondNames(false).Contains(words[1]);
            var thirdgood = PossibleNames.ThirdNames(false).Contains(words[2]);
            if (!firstgood || !secondgood || !thirdgood)
            {
                throw new ArgumentException($"{words[0]} {words[1]} {words[2]} is not a valid name");
            }

            (First, Second, Third) = (words[0], words[1], words[2]);
        }

        public override string ToString()
        {
            return $"{First} {Second} {Third}";
        }

        public static bool operator ==(Name left, Name right) => left.Equals(right);

        public static bool operator !=(Name left, Name right) => !left.Equals(right);

        public override bool Equals(object obj)
        {
            return obj is Name name &&
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