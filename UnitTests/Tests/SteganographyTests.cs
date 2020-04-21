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
using RoslynObfuscator.Obfuscation.Cryptography;
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

            SyntaxTree resultTree = obfuscator.HideLongStringLiteralsInResource(longBadStringTree);
            Console.WriteLine(resultTree);
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

            byte[] payload = Encoding.UTF8.GetBytes("z" + longString);

            byte[] garbageWavNoXor = AudioSteganography.GenerateGarbageWAVFileForPayload(payload, AudioSteganography.PayloadObfuscationMethod.None);
            byte[] garbageWavXor = AudioSteganography.GenerateGarbageWAVFileForPayload(payload,
                AudioSteganography.PayloadObfuscationMethod.XOR);

            Assert.AreNotEqual(garbageWavXor, garbageWavNoXor);

            byte[] retrievedPayloadNoXor = Encoding.UTF8.GetBytes(StegoResourceLoader.GetPayloadFromWavFile(garbageWavNoXor));
            byte[] retrievedPayloadXor = Encoding.UTF8.GetBytes(StegoResourceLoader.GetPayloadFromWavFile(garbageWavXor));

            Assert.AreEqual(payload, retrievedPayloadNoXor);
            Assert.AreEqual(payload, retrievedPayloadXor);
        }

        [Test]
        public void GenerateLongStringAssembly()
        {
            Compilation longBadStringCompilation = TestHelpers.GetLongBadStringTestCompilation();
            SyntaxTree longBadStringTree = longBadStringCompilation.SyntaxTrees.First();

            SourceObfuscator obfuscator = new SourceObfuscator();
            SyntaxTree resultTree = obfuscator.ObfuscateNamespaces(longBadStringTree);
            resultTree = obfuscator.HideLongStringLiteralsInResource(resultTree);
            longBadStringCompilation = longBadStringCompilation.ReplaceSyntaxTree(longBadStringTree, resultTree);

            longBadStringCompilation = obfuscator.ObfuscateStringConstants(longBadStringCompilation);
            longBadStringCompilation = obfuscator.ObfuscateIdentifiers(longBadStringCompilation);
            obfuscator.EmitAssembly(longBadStringCompilation, TestHelpers.AssemblyDirectory + Path.DirectorySeparatorChar + "LongString.exe");
        }
    }
}
