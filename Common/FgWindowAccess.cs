using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Common
{
    public class FgWindowAccess
    {
        public const string FgProcessName = "FallGuys_client_game";

        public static FgStatus GetFgStatus()
        {
            if (!IsFgRunning())
            {
                return FgStatus.NotRunning;
            }
            if (!IsFgInForeGround())
            {
                return FgStatus.RunningButNoFocus;
            }
            return FgStatus.Foreground;
        }

        public static bool IsFgInForeGround()
        {
            var fgWindow = GetForegroundWindow();
            GetWindowThreadProcessId(fgWindow, out var foregroundProcessId);
            var processFallguys = GetFallGuysProcessHandle();
            return foregroundProcessId == processFallguys;
        }

        public static bool IsFgRunning()
        {
            return GetFallGuysProcessHandle() != -1;
        }

        public static WindowPosition GetPositionShit()
        {
            var hwnd = GetFallGuysWindowHandle();

            GetWindowRect(hwnd, out RECT rect);

            return new WindowPosition(rect.Left, rect.Right, rect.Top, rect.Bottom);
        }

        private static int GetFallGuysProcessHandle()
        {
            var processFallguys = Process.GetProcessesByName(FgProcessName);
            if (processFallguys.Length != 1)
            {
                return -1;
            }
            return processFallguys[0].Id;
        }

        private static IntPtr GetFallGuysWindowHandle()
        {
            var processFallguys = Process.GetProcessesByName(FgProcessName);
            if (processFallguys.Length != 1)
            {
                throw new InvalidOperationException("Cannot deduce FallGuys Window when it's not running");
            }
            return processFallguys[0].MainWindowHandle;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}