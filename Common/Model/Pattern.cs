using System;
using System.ComponentModel;
using System.Linq;

namespace Common.Model
{
    public struct Pattern
    {

        public string First { get; }
        public string Second { get; }
        public string Third { get; }

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
                throw new ArgumentException($"{splitted[0]} {splitted[1]} {splitted[2]} is not a viable pattern");
            }
            First = splitted[0];
            Second = splitted[1];
            Third = splitted[2];
        }

        public Pattern(string first, string second, string third)
            : this(new string[] { first, second, third })
        {
        }

        public override string ToString()
        {
            return $"{First} {Second} {Third}";
        }
    }
}