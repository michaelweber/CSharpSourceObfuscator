using System;

namespace ObfuscatorUnitTests.Tests.TestCases
{
    public class MultiFileTest2
    {
        public const string ConstantProperty = "CONSTANT";
        public static string StaticProperty = "STATIC";

        public static string StaticPublicMethod()
        {
            return StaticProperty;
        }

        public string PublicMethod()
        {
            return ConstantProperty;
        }


    }
}
