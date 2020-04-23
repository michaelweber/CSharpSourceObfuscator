using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using ObfuscatorUnitTests.Tests.TestCases;
using RoslynObfuscator.Obfuscation;


namespace ObfuscatorUnitTests.Tests
{
    [TestFixture]
    class SourceObfuscatorTests
    {
        private SourceObfuscator obfuscator;


        [SetUp]
        public void SetupFunction()
        {
            obfuscator = new SourceObfuscator();
        }

        [Test]
        public void FullObfuscateTest()
        {
            Compilation compilation = TestHelpers.GetSharpDumpCompilation();
            SyntaxTree tree = compilation.SyntaxTrees.First();
            SyntaxTree oldTree = tree;
            tree = obfuscator.Obfuscate(tree, compilation);
            string treeString = tree.ToString();
            Console.WriteLine(treeString);
        }

        [Test]
        public void ReplaceUsingTest()
        {
            Compilation replaceUsingCompilation = TestHelpers.GetUsingReplaceTestCompilation();
            SyntaxTree tree = replaceUsingCompilation.SyntaxTrees.First();

            tree = obfuscator.ObfuscateTypeReferences(tree, replaceUsingCompilation, new List<Type>()
            {
                typeof(GZipStream)
            });

            var treeString = tree.ToString();

            Console.WriteLine(treeString);

            //Make sure the actual type references are removed
            Assert.IsFalse(treeString.Contains("GZipStream gZipStream"));
            Assert.IsFalse(treeString.Contains("new GZipStream("));
            Assert.IsFalse(treeString.Contains("gZipStream.Write("));

            //Make sure the other types aren't touched
            Assert.IsTrue(treeString.Contains("(GZipStream())"));
            Assert.IsTrue(treeString.Contains("\"GZipStream\""));
            Assert.IsTrue(treeString.Contains("string GZipStream()"));
        }

        [Test]
        public void TestStringLiteralEncryption()
        {
            Compilation compilation = TestHelpers.GetSharpDumpCompilation();
            SyntaxTree tree = compilation.SyntaxTrees.First();

            tree = obfuscator.ObfuscateStringConstants(tree);
            Console.WriteLine(tree);
            var treeString = tree.ToString();

            //Make sure we injected in the StringEncryptor class
            Assert.IsTrue(treeString.Contains("class StringEncryptor"));

            //Make sure the original program is still there
            Assert.IsTrue(treeString.Contains("class Program"));
            
            //Make sure the extern fields haven't been mucked with
            Assert.IsTrue(treeString.Contains("dbghelp.dll"));
            Assert.IsTrue(treeString.Contains("MiniDumpWriteDump"));

            //Make sure a suspicious string like lsass has been encrypted
            Assert.IsFalse(treeString.Contains("lsass"));

            //The RANDOMIZEME should have been replaced
            Assert.IsFalse(treeString.Contains("Key = \"RANDOMIZEME\""));

            int decryptionOccurences = treeString.Select((c, i) => treeString.Substring(i))
                .Count(sub => sub.StartsWith("StringEncryptor.Decrypt"));
            Assert.AreEqual(29, decryptionOccurences);
        }

        [Test]
        public void OnlyReplaceFunctionNamesWhenRenamingFunctions1()
        {
            Compilation compilation = TestHelpers.GetRenamingTestCompilation();
            SyntaxTree tree = compilation.SyntaxTrees.First();

            tree = obfuscator.ObfuscateIdentifiers(tree, compilation);
            string treeString = tree.ToString();
            Console.WriteLine(treeString);

            //Make sure that the "Compress" token in CompressionMode.Compress wasn't renamed
            Assert.IsTrue(treeString.Contains("CompressionMode.Compress"));

            //Make sure that the Entry Point is left intact
            Assert.IsTrue(treeString.Contains("void Main(string[]"));
        }

        [Test]
        public void OnlyReplaceFunctionNamesWhenRenamingFunctions2()
        {
            Compilation compilation = TestHelpers.GetSharpDumpCompilation();
            SyntaxTree tree = compilation.SyntaxTrees.First();

            tree = obfuscator.ObfuscateIdentifiers(tree, compilation);
            string treeString = tree.ToString();
            Console.WriteLine(treeString);

            //Make sure that the "Compress" token in CompressionMode.Compress wasn't renamed
            Assert.IsTrue(treeString.Contains("CompressionMode.Compress"));

            //Make sure that the Entry Point is left intact
            Assert.IsTrue(treeString.Contains("void Main(string[]"));
        }

        [Test]
        public void TestSimplePInvokeReplacement()
        {
            Compilation compilation = TestHelpers.GetPinvokeSimpleTestCompilation();
            // SyntaxTree tree = compilation.SyntaxTrees.First();
            compilation = obfuscator.ObfuscatePInvokeCalls(compilation);
            compilation = obfuscator.HideLongStringLiteralsInResource(compilation);
            compilation = obfuscator.ObfuscateStringConstants(compilation);
            compilation = obfuscator.ObfuscateNamespaces(compilation);
            compilation = obfuscator.ObfuscateIdentifiers(compilation);

            string treeString = compilation.SyntaxTrees.First().ToString();
            Console.WriteLine(treeString);
            obfuscator.EmitAssembly(compilation,
                TestHelpers.AssemblyDirectory + Path.DirectorySeparatorChar + "pinvoke.exe");
        }


    }
}
