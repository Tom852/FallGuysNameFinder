using Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallGuysNameFinder
{
    public class AddPatternViewModel
    {
        public Pattern Pattern { get; set; }
        public List<string> FirstNames { get; set; }
        public List<string> SecondNames { get; set; }
        public List<string> ThirdNames { get; set; }

    }
}
