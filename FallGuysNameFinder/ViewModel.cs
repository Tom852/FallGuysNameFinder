using Backend;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallGuysNameFinder
{
    public class ViewModel
    {

        public ObservableCollection<Pattern> Patterns { get; set; }
        public Options Options { get; set; }

    }
}
