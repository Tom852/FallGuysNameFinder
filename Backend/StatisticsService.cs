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
        public void Account(History history)
        {
            var stats = DataStorageStuff.GetStats();
            stats.Account(history.GetWithoutSameElementsInRow());
            DataStorageStuff.SaveStats(stats);
        }
    }
}
