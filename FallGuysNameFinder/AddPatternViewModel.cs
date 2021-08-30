using Common.Model;
using System.Collections.Generic;

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