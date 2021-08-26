using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ForeGroundWindowChecker
    {
        public static bool IsFgInForeGround()
        {
            var fgWindow = GetForegroundWindow();
            GetWindowThreadProcessId(fgWindow, out var foregroundProcessId);
            var processFallguys = GetFallGuysHandle();
            return foregroundProcessId == processFallguys;
        }

        public static bool IsFgRunning()
        {
            return GetFallGuysHandle() != -1;
        }

        private static int GetFallGuysHandle()
        {
            var processFallguys = Process.GetProcessesByName("FallGuys_client_game");
            if (processFallguys.Length != 1) {
                return -1;
            }
            return processFallguys[0].Id;


        }


        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}
