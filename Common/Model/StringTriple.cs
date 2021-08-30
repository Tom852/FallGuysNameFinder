using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public struct StringTriple
    {
        public string First { get; set; }
        public string Second { get; set; }
        public string Third { get; set; }

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
