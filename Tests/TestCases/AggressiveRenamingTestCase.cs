using System;
using System.IO.Compression;

namespace RoslynObfuscator.Tests.TestCases
{
    public class AggressiveRenamingTest
    {
        public static CompressionMode Compress()
        {
            return CompressionMode.Compress;
        }

        public static void Main(string[] args)
        {
            string Compress = "Hello";

            Console.WriteLine(AggressiveRenamingTest.Compress());
            Console.WriteLine(Compress);
            Console.WriteLine(CompressionMode.Compress);
        }
    }
}