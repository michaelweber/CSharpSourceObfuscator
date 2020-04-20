using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace RoslynObfuscator.Obfuscation.InjectedClasses
{
    /***
     * Taken from https://ourcodeworld.com/articles/read/474/getting-started-with-steganography-hide-information-on-images-with-c-sharp
     */
    public static class SteganographyHelper
    {
        public static byte[] GenerateGarbageWAVFileForPayload(byte[] payload)
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

            wavWriter.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            wavWriter.Write(36 + numSamples * numChannels * sampleLength);
            wavWriter.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            wavWriter.Write(16);
            wavWriter.Write((ushort)1);
            wavWriter.Write(numChannels);
            wavWriter.Write(sampleRate);
            wavWriter.Write(sampleRate * sampleLength * numChannels);
            wavWriter.Write((ushort)(sampleLength * numChannels));
            wavWriter.Write((ushort)(8 * sampleLength));
            wavWriter.Write(Encoding.ASCII.GetBytes("data"));
            wavWriter.Write(numSamples * sampleLength);
            wavWriter.Write(payload);

            wavWriter.Close();

            byte[] res = ms.ToArray();
            return res;
        }

        // 
        // enum State
        // {
        //     HIDING,
        //     FILL_WITH_ZEROS
        // };
        //
        // /// <summary>
        // /// Creates a bitmap from an image without indexed pixels
        // /// </summary>
        // /// <param name="src"></param>
        // /// <returns></returns>
        // public static Bitmap CreateNonIndexedImage(Image src)
        // {
        //     Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //
        //     using (Graphics gfx = Graphics.FromImage(newBmp))
        //     {
        //         gfx.DrawImage(src, 0, 0);
        //     }
        //
        //     return newBmp;
        // }
        //
        // public static Bitmap MergeText(string text, Bitmap bmp)
        // {
        //     State s = State.HIDING;
        //
        //     int charIndex = 0;
        //     int charValue = 0;
        //     long colorUnitIndex = 0;
        //
        //     int zeros = 0;
        //
        //     int R = 0, G = 0, B = 0;
        //
        //     for (int i = 0; i < bmp.Height; i++)
        //     {
        //         for (int j = 0; j < bmp.Width; j++)
        //         {
        //             Color pixel = bmp.GetPixel(j, i);
        //
        //             pixel = Color.FromArgb(pixel.R - pixel.R % 2,
        //                 pixel.G - pixel.G % 2, pixel.B - pixel.B % 2);
        //
        //             R = pixel.R; G = pixel.G; B = pixel.B;
        //
        //             for (int n = 0; n < 3; n++)
        //             {
        //                 if (colorUnitIndex % 8 == 0)
        //                 {
        //                     if (zeros == 8)
        //                     {
        //                         if ((colorUnitIndex - 1) % 3 < 2)
        //                         {
        //                             bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
        //                         }
        //
        //                         return bmp;
        //                     }
        //
        //                     if (charIndex >= text.Length)
        //                     {
        //                         s = State.FILL_WITH_ZEROS;
        //                     }
        //                     else
        //                     {
        //                         charValue = text[charIndex++];
        //                     }
        //                 }
        //
        //                 switch (colorUnitIndex % 3)
        //                 {
        //                     case 0:
        //                         {
        //                             if (s == State.HIDING)
        //                             {
        //                                 R += charValue % 2;
        //
        //                                 charValue /= 2;
        //                             }
        //                         }
        //                         break;
        //                     case 1:
        //                         {
        //                             if (s == State.HIDING)
        //                             {
        //                                 G += charValue % 2;
        //
        //                                 charValue /= 2;
        //                             }
        //                         }
        //                         break;
        //                     case 2:
        //                         {
        //                             if (s == State.HIDING)
        //                             {
        //                                 B += charValue % 2;
        //
        //                                 charValue /= 2;
        //                             }
        //
        //                             bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
        //                         }
        //                         break;
        //                 }
        //
        //                 colorUnitIndex++;
        //
        //                 if (s == State.FILL_WITH_ZEROS)
        //                 {
        //                     zeros++;
        //                 }
        //             }
        //         }
        //     }
        //
        //     return bmp;
        // }
        //
        // public static string ExtractText(Bitmap bmp)
        // {
        //     int colorUnitIndex = 0;
        //     int charValue = 0;
        //
        //     string extractedText = String.Empty;
        //
        //     for (int i = 0; i < bmp.Height; i++)
        //     {
        //         for (int j = 0; j < bmp.Width; j++)
        //         {
        //             Color pixel = bmp.GetPixel(j, i);
        //
        //             for (int n = 0; n < 3; n++)
        //             {
        //                 switch (colorUnitIndex % 3)
        //                 {
        //                     case 0:
        //                         {
        //                             charValue = charValue * 2 + pixel.R % 2;
        //                         }
        //                         break;
        //                     case 1:
        //                         {
        //                             charValue = charValue * 2 + pixel.G % 2;
        //                         }
        //                         break;
        //                     case 2:
        //                         {
        //                             charValue = charValue * 2 + pixel.B % 2;
        //                         }
        //                         break;
        //                 }
        //
        //                 colorUnitIndex++;
        //
        //                 if (colorUnitIndex % 8 == 0)
        //                 {
        //                     charValue = reverseBits(charValue);
        //
        //                     if (charValue == 0)
        //                     {
        //                         return extractedText;
        //                     }
        //
        //                     char c = (char)charValue;
        //
        //                     extractedText += c.ToString();
        //                 }
        //             }
        //         }
        //     }
        //
        //     return extractedText;
        // }
        //
        // public static int reverseBits(int n)
        // {
        //     int result = 0;
        //
        //     for (int i = 0; i < 8; i++)
        //     {
        //         result = result * 2 + n % 2;
        //
        //         n /= 2;
        //     }
        //
        //     return result;
        // }
    }
}
