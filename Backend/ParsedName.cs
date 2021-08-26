using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class ParsedName
    {
        public string First { get; set; }
        public string Second { get; set; }
        public string Third { get; set; }

        public ParsedName(string s)
            : this(s.Split(' '))
        {
        }
        public ParsedName(string[] splitted)
        {
            if (splitted.Length != 3)
            {
                throw new Exception("Name must contain of 3 words separated by space.");
            }

            First = splitted[0];
            Second = splitted[1];
            Third = splitted[2];

        }
    }
}
