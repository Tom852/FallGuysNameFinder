using Common;
using Common.Model;
using Serilog;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Backend
{
    public class StatisticsService
    {
        public void Encount(History history)
        {
            var dss = new DataStorageStuff();
            var stats = dss.GetStats();
            stats.Encount(history.PreviousNames);
            dss.SaveStats(stats);
        }
    }
}
