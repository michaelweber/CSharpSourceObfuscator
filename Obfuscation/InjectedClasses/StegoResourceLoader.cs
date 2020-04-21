using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Reflection;
using System.Resources;
using System.Text;

namespace RoslynObfuscator.Obfuscation.InjectedClasses
{
    /***
     * Taken from https://ourcodeworld.com/articles/read/474/getting-started-with-steganography-hide-information-on-images-with-c-sharp
     */
    public static class StegoResourceLoader
    {
        public static byte[] GetResourceBytes(string streamName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string resourceName = asm.GetName().Name + ".Properties.Resources";
            var rm = new ResourceManager(resourceName, asm);
            Stream s = rm.GetStream(streamName);
            SoundPlayer sp = new SoundPlayer(s);
            byte[] buffer = new byte[sp.Stream.Length];
            int bytesRead = sp.Stream.Read(buffer, 0, (int)sp.Stream.Length);
            return buffer;
        }

        public static string GetPayloadFromWavFile(byte[] wavBytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(wavBytes));
            byte[] wavHeaderContent = reader.ReadBytes(44);
            ushort payloadType = reader.ReadUInt16();

            if (payloadType == 0)
            {
                return Encoding.UTF8.GetString(reader.ReadBytes(wavBytes.Length - 44 - 2));
            }
            if (payloadType == 1)
            {
                ushort keyLen = reader.ReadUInt16();
                byte[] key = reader.ReadBytes(keyLen);
                byte[] xPayload = reader.ReadBytes(wavBytes.Length - 44 - 2 - 2 - keyLen);
                byte[] payload = XorBytesWithKey(xPayload, key);
                return Encoding.UTF8.GetString(payload);
            }
            
            throw new NotImplementedException();
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
