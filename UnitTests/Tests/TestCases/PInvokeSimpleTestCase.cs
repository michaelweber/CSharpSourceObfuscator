using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ObfuscatorUnitTests.Tests.TestCases
{
    public class PInvokeSimpleTestCase
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr MessageBox(int hWnd, String text,
            String caption, uint type);
        
        public static void Main()
        {
            MessageBox(0, "Hello World", "Platform Invoke Sample", 0);
        }

    }
}
