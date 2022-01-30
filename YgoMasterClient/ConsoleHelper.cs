using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace YgoMasterClient
{
    class ConsoleHelper
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(UInt32 nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern void SetStdHandle(UInt32 nStdHandle, IntPtr handle);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private const UInt32 StdOutputHandle = 0xFFFFFFF5;

        private static IntPtr consoleHandle;
        private static TextWriter output;

        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;

        private static string title;
        public static string Title
        {
            get
            {
                return consoleHandle == IntPtr.Zero ? title : title = Console.Title;
            }
            set
            {
                title = value;
                if (consoleHandle != IntPtr.Zero)
                {
                    Console.Title = value;
                }
            }
        }

        static ConsoleHelper()
        {
            output = new ConsoleTextWriter();
            Console.SetOut(output);

            consoleHandle = GetConsoleWindow();
            if (consoleHandle != IntPtr.Zero)
            {
                Console.Title = Title;
            }
        }

        public static bool IsConsoleVisible
        {
            get { return (consoleHandle = GetConsoleWindow()) != IntPtr.Zero && IsWindowVisible(consoleHandle); }
            //get { return (consoleHandle = GetConsoleWindow()) != IntPtr.Zero; }
        }

        public static void ToggleConsole()
        {
            consoleHandle = GetConsoleWindow();
            if (consoleHandle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                FreeConsole();
            }
        }

        public static void ShowConsole()
        {
            consoleHandle = GetConsoleWindow();
            if (consoleHandle == IntPtr.Zero)
            {
                AllocConsole();
                consoleHandle = GetConsoleWindow();
            }
            else
            {
                ShowWindow(consoleHandle, SW_SHOW);
            }

            if (consoleHandle != IntPtr.Zero)
            {
                Console.Title = title != null ? title : string.Empty;
            }
        }

        public static void HideConsole()
        {
            consoleHandle = GetConsoleWindow();
            if (consoleHandle != IntPtr.Zero)
            {
                ShowWindow(consoleHandle, SW_HIDE);
            }
        }

        public static void CloseConsole()
        {
            consoleHandle = GetConsoleWindow();
            if (consoleHandle != IntPtr.Zero)
            {
                FreeConsole();
            }
        }
    }

    public class ConsoleTextWriter : TextWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }

        // TODO: WriteConsole may not write all the data, chunk this data into several calls if nessesary

        // WriteConsoleW issues reference:
        // https://svn.apache.org/repos/asf/logging/log4net/tags/log4net-1_2_9/src/Appender/ColoredConsoleAppender.cs

        public override void Write(string value)
        {
            uint written;
            if (!WriteConsoleW(new IntPtr(7), value, (uint)value.Length, out written, IntPtr.Zero) || written < value.Length)
            {
                if (GetConsoleWindow() != IntPtr.Zero)
                {
                    //System.Diagnostics.Debugger.Break();
                }
            }
        }

        public override void WriteLine(string value)
        {
            value = value + Environment.NewLine;
            uint written;
            if (!WriteConsoleW(new IntPtr(7), value, (uint)value.Length, out written, IntPtr.Zero) || written < value.Length)
            {
                if (GetConsoleWindow() != IntPtr.Zero)
                {
                    //System.Diagnostics.Debugger.Break();
                }
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool WriteConsoleW(IntPtr hConsoleOutput, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer,
           uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
           IntPtr lpReserved);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool WriteConsole(IntPtr hConsoleOutput, string lpBuffer,
           uint nNumberOfCharsToWrite, out uint lpNumberOfCharsWritten,
           IntPtr lpReserved);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCP(int wCodePageID);

        [DllImport("kernel32.dll")]
        static extern uint GetACP();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
    }
}
