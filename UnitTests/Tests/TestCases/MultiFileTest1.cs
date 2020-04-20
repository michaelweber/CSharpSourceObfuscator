using System;

namespace ObfuscatorUnitTests.Tests.TestCases
{
    public class MultiFileTest1
    {
        public void UseMultiFileTest2()
        {
            if (MultiFileTest2.StaticProperty == MultiFileTest2.StaticPublicMethod())
            {
                MultiFileTest2 mft2 = new MultiFileTest2();
                if (MultiFileTest2.ConstantProperty == mft2.PublicMethod())
                {
                    Console.WriteLine("Success!");
                }
            }
        }
    }
}
