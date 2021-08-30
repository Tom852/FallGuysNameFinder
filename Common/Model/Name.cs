using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
    public class Name
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
    }
}
