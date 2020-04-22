using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using RoslynObfuscator.Obfuscation.InjectedClasses;

namespace ObfuscatorUnitTests.Tests
{
    [TestFixture]
    class PInvokeLoaderTests
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

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

        [Test]
        public void TestMessageBoxTransform()
        {
            string messageBoxSig = "user32.dll|MessageBox|IntPtr|int hWnd|String text|String caption|uint type";
            object[] args = {0, "Hello World", "Platform Invoke Sample", 0};
            PInvokeLoader.Instance.InvokePInvokeFunction(messageBoxSig, args);
        }

        [Test]
        public void TestGetWindowTransforms()
        {
            Process p = Process.Start("notepad");

            IntPtr winHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Notepad", "Untitled - Notepad");

            for (int wait = 0; wait < 10; wait += 1)
            {
                //Wait a second for calc to load
                Thread.Sleep(1000);
                if (winHandle != IntPtr.Zero) break;
                winHandle = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Notepad", "Untitled - Notepad");
            }

            Assert.AreNotEqual(0, winHandle);

            string messageBoxSig = "user32.dll|FindWindowEx|IntPtr|IntPtr hwndParent|IntPtr hwndChildAFter|string lpszClass|string lpszWindow";
            object[] args = {IntPtr.Zero, IntPtr.Zero, "Notepad", "Untitled - Notepad" };
            IntPtr response = (IntPtr) PInvokeLoader.Instance.InvokePInvokeFunction(messageBoxSig, args);
            Console.WriteLine("P/Invoke Result: {0}", winHandle);
            Console.WriteLine("Dynamic P/Invoke Result: {0}", response);

            p.Kill();

            Assert.AreEqual(winHandle, response);
        }

        [Test]
        public void TestPassByRefInvoke()
        {
            SYSTEMTIME test1;
            MethodInfo[] methods = typeof(PInvokeLoaderTests).GetMethods();

            GetSystemTime(out test1);

            string getSystemTimeSig = "kernel32.dll|GetSystemTime|void|out SYSTEMTIME lpSystemTime";
            object[] args = new Object[] {new SYSTEMTIME()};
            PInvokeLoader.Instance.InvokePInvokeFunction(getSystemTimeSig, args);
            SYSTEMTIME test2 = (SYSTEMTIME)args[0];

            Assert.AreEqual(test1.Year, test2.Year);
            Assert.AreEqual(test1.Month, test2.Month);
            Assert.AreEqual(test1.Day, test2.Day);
            Assert.AreEqual(test1.Hour, test2.Hour);
        }
    }
}
