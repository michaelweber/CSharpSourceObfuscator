using System;
using System.Collections.Generic;
using System.Text;

namespace RoslynObfuscator.Obfuscation.InjectedClasses
{
    public static class StringEncryptor
    {
        public static string Key = "RANDOMIZEME";

        public static byte[] XorString(string xorString)
        {
            return XorBytes(Encoding.UTF8.GetBytes(xorString));
        }

        public static byte[] XorBytes(byte[] bytes)
        {
            var result = new List<byte>();

            for (int i = 0; i < (bytes.Length); i++)
            {
                result.Add((byte)((uint)bytes[i] ^ (uint)Key[i % Key.Length]));
            }

            return result.ToArray();
        }


        public static string EncryptString(string secret)
        {
            return Convert.ToBase64String(XorString(secret));
        }

        public static string DecryptString(string cipherText)
        {
            return Encoding.UTF8.GetString(XorBytes(Convert.FromBase64String(cipherText)));
        }

    }
}
