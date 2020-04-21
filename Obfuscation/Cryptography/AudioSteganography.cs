using System;
using System.IO;
using System.Text;

namespace RoslynObfuscator.Obfuscation.Cryptography
{
    //Steganography/Cryptography is used here in the loosest of terms, we just create a WAV file with
    //the right headers and jam the content into the streams
    public static class AudioSteganography
    {
        public enum PayloadObfuscationMethod
        {
            None = 0,
            XOR = 1
        }
        public static byte[] GenerateGarbageWAVFileForPayload(byte[] payload, PayloadObfuscationMethod method = PayloadObfuscationMethod.XOR)
        {
            //Code adapted from https://stackoverflow.com/questions/14659684/creating-a-wav-file-in-c-sharp
            //and https://www.researchgate.net/figure/The-structure-of-wav-file-format_fig1_273630623

            //The WAV header is going to be 44 bytes
            MemoryStream ms = new MemoryStream();
            BinaryWriter wavWriter = new BinaryWriter(ms);
            ushort numChannels = 2;
            ushort sampleLength = 1;
            uint numSamples = Convert.ToUInt32(payload.Length);
            uint sampleRate = 22050;

            int obfuscationMethodByteSize = 2;
            int xorKeyLen = 20;

            if (method == PayloadObfuscationMethod.XOR)
            {
                obfuscationMethodByteSize += xorKeyLen + 2;
            }


            //Note, if we don't explicitly convert to UInt16 or 32, then 
            //the byte sizes will be wrong for 64 bit machines
            wavWriter.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            wavWriter.Write(Convert.ToUInt32(36 + numSamples * numChannels * sampleLength + obfuscationMethodByteSize));
            wavWriter.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            wavWriter.Write(Convert.ToUInt32(16));
            wavWriter.Write(Convert.ToUInt16(1));
            wavWriter.Write(Convert.ToUInt16(numChannels));
            wavWriter.Write(Convert.ToUInt32(sampleRate));
            wavWriter.Write(Convert.ToUInt32(sampleRate * sampleLength * numChannels));
            wavWriter.Write(Convert.ToUInt16(sampleLength * numChannels));
            wavWriter.Write(Convert.ToUInt16(8 * sampleLength));
            wavWriter.Write(Encoding.ASCII.GetBytes("data"));
            wavWriter.Write(Convert.ToUInt32(numSamples * sampleLength + obfuscationMethodByteSize));


            //Write our method
            wavWriter.Write(Convert.ToUInt16(method));

            switch (method)
            {
                case PayloadObfuscationMethod.XOR:
                {
                    byte[] key = XorCrypto.GetRandomXorKey(xorKeyLen);
                    wavWriter.Write(Convert.ToUInt16(xorKeyLen));
                    wavWriter.Write(key);
                    byte[] xoredBytes = XorCrypto.XorBytesWithKey(payload, key);
                    wavWriter.Write(xoredBytes);
                    break;
                }
                case PayloadObfuscationMethod.None:
                {
                    wavWriter.Write(payload);
                    break;
                }
                default:
                    throw new NotImplementedException();
            }

            wavWriter.Close();

            byte[] res = ms.ToArray();
            return res;
        }

        
    }
}
