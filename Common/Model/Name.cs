using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Model
{
    // Although being the same as StringTriple, I think the semantic differntiation between the two makes sense.
    // Could add a check if the name really exists and correct possible casing issues theoretically but i skip it for now. (would be good for fail fast principle)

    public struct Name
    {
        public string First { get; set; }
        public string Second { get; set; }
        public string Third { get; set; }

        public Name(string first, string second, string third) => (First, Second, Third) = (first, second, third);

        public Name(IEnumerable<string> words)
            : this(words.ToArray())
        {
        }

        public Name(string[] words)
        {
            if (words.Length != 3)
            {
                throw new ArgumentException("Cannot construct name out of != 3 words");
            }
            First = words[0];
            Second = words[1];
            Third = words[2];
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