using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynObfuscator.Obfuscation.Cryptography
{
    public static class XorCrypto
    {
        public static byte[] GetRandomXorKey(int desiredLength)
        {
            Random r = new Random();
            byte[] key = new byte[desiredLength];
            r.NextBytes(key);
            return key;
        }

        public static byte[] XorBytesWithKey(byte[] bytes, byte[] key)
        {
            var result = new List<byte>();

            for (int i = 0; i < (bytes.Length); i++)
            {
                result.Add((byte)((uint)bytes[i] ^ (uint)key[i % key.Length]));
            }

            return result.ToArray();
        }
    }
}
