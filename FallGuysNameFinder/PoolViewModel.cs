using Common;
using Common.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace FallGuysNameFinder
{
    public class PoolViewModel
    {
        public string[] FirstNames { get; } = PossibleNames.FirstNames(false);
        public string[] SecondNames { get; } = PossibleNames.SecondNames(false);
        public string[] ThirdNames { get; } = PossibleNames.ThirdNames(false);
    }
}