using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class Name
    {
        public string First { get; set; }
        public string Second { get; set; }
        public string Third { get; set; }

        public Name(string s)
            : this(s.Split(' '))
        {
        }
        public Name(string[] splitted)
        {
            if (splitted.Length != 3)
            {
                Log.Warning("Name should contain of 3 words separated by space. First 3 will be taken.");
            }

            // todo: Great FallGuy fixen
            First = splitted[0];
            Second = splitted[1];
            Third = splitted[2];

        }
    }
}
