using System;
using System.IO.Compression;
using System.Text;

namespace ObfuscatorUnitTests.Tests.TestCases
{
    class UsingReplaceCase
    {
        public static byte[] Payload = Encoding.UTF8.GetBytes("Payload");
        public void ForbiddenUsingMethod()
        {
            var test = 1234;
            using (GZipStream gZipStream = new GZipStream(null, CompressionLevel.Fastest))
            {
                gZipStream.Write(Payload, 0, Payload.Length);
            }

            Console.WriteLine(GZipStream());
        }

        private string GZipStream()
        {
            string redHerring = "GZipStream";
            return redHerring;
        }
    }
}
