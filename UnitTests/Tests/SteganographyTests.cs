using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ObfuscatorUnitTests.Tests.TestCases;
using RoslynObfuscator.Obfuscation;
using RoslynObfuscator.Obfuscation.InjectedClasses;

namespace ObfuscatorUnitTests.Tests
{
    [TestFixture]
    class SteganographyTests
    {
        [Test]
        public void TestLongStringStego()
        {
            Compilation longBadStringCompilation = TestHelpers.GetLongBadStringTestCompilation();
            SyntaxTree longBadStringTree = longBadStringCompilation.SyntaxTrees.First();

            Image image = TestHelpers.GetStegoImage();
            SourceObfuscator obfuscator = new SourceObfuscator();

            SyntaxTree resultTree = obfuscator.HideLongStringLiteralsInImage(longBadStringTree, image);
        }

        [Test]
        public void TestAudioStego()
        {
            Compilation longBadStringCompilation = TestHelpers.GetLongBadStringTestCompilation();
            SyntaxTree longBadStringTree = longBadStringCompilation.SyntaxTrees.First();

            List<SyntaxToken> longStringLiteralTokens =
                (from token in longBadStringTree.GetRoot().DescendantTokens()
                    where token.IsKind(SyntaxKind.StringLiteralToken) && token.ValueText.Length > 1000
                    select token).ToList();

            string longString = longStringLiteralTokens.First().ValueText;

            StringEncryptor.Key = "TESTKEY";
            byte[] encryptedPayload = StringEncryptor.XorBytes(Convert.FromBase64String("z" + longString));

            byte[] garbageWav = SteganographyHelper.GenerateGarbageWAVFileForPayload(encryptedPayload);
            File.WriteAllBytes(@"S:\projects\malware-dropper\RoslynObfuscator\garbage.wav", garbageWav);
            Assert.AreEqual(encryptedPayload, garbageWav.Skip(44).ToArray());
        }
    }
}
