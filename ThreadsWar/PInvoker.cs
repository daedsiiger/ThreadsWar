using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ThreadsWar
{
    //public static class PInvokerExtensions
    //{
    //    public static uint WriteAt(this IntPtr consoleWindow, string str, short x, short y)
    //    {
    //        PInvoker.WriteConsoleOutputCharacter(consoleWindow, str, 1, new PInvoker.COORD(x, y), out var res);
    //        return res;
    //    }
    //}

    public class PInvoker
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        public enum VirtualKeys
        : ushort
        {
            Space = 0x20,
            Left = 0x25,
            Right = 0x27,
            None = 0x999,
        }

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct KEY_EVENT_RECORD
        {
            [FieldOffset(0), MarshalAs(UnmanagedType.Bool)]
            public bool bKeyDown;
            [FieldOffset(4), MarshalAs(UnmanagedType.U2)]
            public ushort wRepeatCount;
            [FieldOffset(6), MarshalAs(UnmanagedType.U2)]
            public VirtualKeys wVirtualKeyCode;
            [FieldOffset(8), MarshalAs(UnmanagedType.U2)]
            public ushort wVirtualScanCode;
            [FieldOffset(10)]
            public char UnicodeChar;
        }

        public struct WINDOW_BUFFER_SIZE_RECORD
        {
            public COORD dwSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MENU_EVENT_RECORD
        {
            public uint dwCommandId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FOCUS_EVENT_RECORD
        {
            public uint bSetFocus;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUT_RECORD_UNION
        {
            [FieldOffset(0)]
            public KEY_EVENT_RECORD KeyEvent;
            [FieldOffset(0)]
            public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;
            [FieldOffset(0)]
            public MENU_EVENT_RECORD MenuEvent;
            [FieldOffset(0)]
            public FOCUS_EVENT_RECORD FocusEvent;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT_RECORD
        {
            public ushort EventType;
            public INPUT_RECORD_UNION Event;
            public const ushort KEY_EVENT = 0x0001;
        };

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetStdHandle(int nStdHandle);


        //[DllImport("kernel32.dll", SetLastError = true)]
        //public static extern bool WriteConsoleOutputCharacter(
        //    IntPtr hConsoleOutput,
        //    string lpCharacter,
        //    uint nLength,
        //    COORD dwWriteCoord,
        //    out uint lpNumberOfCharsWritten
        //);

        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ReadConsoleInput(
            IntPtr hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsoleOutputCharacter(
            IntPtr hConsoleOutput,
            [Out] char[] lpCharacter,
            int nLength,
            COORD dwReadCoord,
            out int lpNumberOfCharsRead
        );

    }
}
