using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    public class Options
    {
        public bool StopOnAlliteration { get; set; }
        public bool StopOnDoubleWord { get; set; }
        public bool WaitOnError { get; set; }
    }
}
