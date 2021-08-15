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
        {
            var splitted = s.Split(' ');
            if (splitted.Length != 3)
            {
                throw new Exception("Name must contain of 3 words separated by space, but it was: " + s);
            }

            First = splitted[0];
            Second = splitted[1];
            Third = splitted[2];

        }
    }
}
