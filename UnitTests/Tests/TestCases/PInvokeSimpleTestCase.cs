using System;
using System.Runtime.InteropServices;

namespace ObfuscatorUnitTests.Tests.TestCases
{
    public class PInvokeSimpleTestCase
    {
        [DllImport("user32.dll", EntryPoint = "MessageBox", CharSet = CharSet.Auto)]
        public static extern IntPtr MessageBoxDelegate(int hWnd, [MarshalAs(UnmanagedType.LPWStr)]String text,
            ref String caption, uint type);

        [DllImport("kernel32.dll")]
        private static extern void GetSystemTime(out SYSTEMTIME lpSystemTime);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)] public short Year;
            [MarshalAs(UnmanagedType.U2)] public short Month;
            [MarshalAs(UnmanagedType.U2)] public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)] public short Day;
            [MarshalAs(UnmanagedType.U2)] public short Hour;
            [MarshalAs(UnmanagedType.U2)] public short Minute;
            [MarshalAs(UnmanagedType.U2)] public short Second;
            [MarshalAs(UnmanagedType.U2)] public short Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                dt = dt.ToUniversalTime();  // SetSystemTime expects the SYSTEMTIME in UTC
                Year = (short)dt.Year;
                Month = (short)dt.Month;
                DayOfWeek = (short)dt.DayOfWeek;
                Day = (short)dt.Day;
                Hour = (short)dt.Hour;
                Minute = (short)dt.Minute;
                Second = (short)dt.Second;
                Milliseconds = (short)dt.Millisecond;
            }
        }

        public static void Main()
        {
            SYSTEMTIME test1;
            GetSystemTime(out test1);
            string caption = "Hello World"; 
            string time = string.Format("It is {0}:{1}:{2}.{3}", test1.Hour, test1.Minute, test1.Second,
                test1.Milliseconds);
            MessageBoxDelegate(0, time, ref caption, 0);
        }
    }
}
