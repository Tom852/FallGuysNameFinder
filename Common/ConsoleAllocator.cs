using System;
using System.Runtime.InteropServices;

namespace Common
{
    //Props: https://stackoverflow.com/questions/31978826/is-it-possible-to-have-a-wpf-application-print-console-output/31978833
    public static class ConsoleAllocator
    {
        [DllImport(@"kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport(@"kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(@"user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SwHide = 0;
        private const int SwShow = 5;

        private const UInt32 StdOutputHandle = 0xFFFFFFF5;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);

        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                ShowWindow(handle, SwShow);
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SwHide);
        }
    }
}