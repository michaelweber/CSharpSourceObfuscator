using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Rename;
using NUnit.Framework;
using ObfuscatorUnitTests.Tests.TestCases;
using RoslynObfuscator;
using RoslynObfuscator.Obfuscation;

namespace ObfuscatorUnitTests.Tests
{
    [TestFixture]
    class SolutionHandlingTests
    {

        [Test]
        public void MultiFileRenamingTest()
        {
            Compilation multiCompilation = TestHelpers.GetMultiFileTestCompilation();
            SourceObfuscator sourceObfuscator = new SourceObfuscator();

            multiCompilation = sourceObfuscator.ObfuscateIdentifiers(multiCompilation);

            foreach (SyntaxTree tree in multiCompilation.SyntaxTrees)
            {
                Console.WriteLine(tree);
            }
        }

        [Test]
        public void MultiFileRenamingStaticRenameTest()
        {
            //This test exists because when a solution is used, for some reason the
            //static Instance property in PInvokeLoader.cs isn't renamed though its usages are

            Compilation compilation = TestHelpers.GetMultiFileTestCompilation();
            SourceObfuscator obfuscator = new SourceObfuscator();

            compilation = obfuscator.ObfuscatePInvokeCalls(compilation);
            compilation = obfuscator.HideLongStringLiteralsInResource(compilation);
            compilation = obfuscator.ObfuscateStringConstants(compilation);
            compilation = obfuscator.ObfuscateNamespaces(compilation);
            compilation = obfuscator.ObfuscateIdentifiers(compilation);

            List<string> syntaxTreeStrings = compilation.SyntaxTrees.Select(tree => tree.ToString()).ToList();

            bool instanceStringStillExists = syntaxTreeStrings.Any(treeString => treeString.Contains("Instance"));
            Assert.IsFalse(instanceStringStillExists);
        }

        [TestCase(ExpectedResult = 1)]
        public async Task<int> LoadSolutionTest()
        {
            Solution safetyKatzSolution = await TestHelpers.GetSafetyKatzSolution();
            var syntaxTrees = await safetyKatzSolution.GetSyntaxTreesAsync();
            Compilation compilation = CSharpCompilation.Create("SafetyKatz");
            compilation = compilation.AddSyntaxTrees(syntaxTrees);
            
            SourceObfuscator obfuscator = new SourceObfuscator();
            compilation = obfuscator.ObfuscateStringConstants(compilation);
            compilation = obfuscator.ObfuscateIdentifiers(compilation);

            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                string treeString = tree.ToString();
                Console.WriteLine(treeString);
            }

            return 1;
        }

        [TestCase(ExpectedResult = true)]
        public async Task<bool> TestIdentifierObfuscationAfterInjectingEncryptor()
        {
            Solution safetyKatzSolution = await TestHelpers.GetSafetyKatzSolution();

            var syntaxTrees = await safetyKatzSolution.GetSyntaxTreesAsync();
            Compilation compilation = CSharpCompilation.Create("SafetyKatz");

            //Only add Constants.cs
            compilation = compilation.AddSyntaxTrees(syntaxTrees.First(tree => tree.FilePath.Contains("Constants.cs")));

            SourceObfuscator obfuscator = new SourceObfuscator();
            compilation = obfuscator.ObfuscateStringConstants(compilation);

            List<ISymbol> symbols =
                CodeIntrospectionHelper.GetAllSymbols(compilation, compilation.SyntaxTrees.First().GetRoot()).ToList();

            compilation = obfuscator.ObfuscateNamespaces(compilation);
            compilation = obfuscator.ObfuscateIdentifiers(compilation);

            SyntaxTree tree = compilation.SyntaxTrees.First();

            string treeString = tree.ToString();
            Console.WriteLine(treeString);
            
            Assert.IsFalse(treeString.Contains("XorBytes"));
            

            return true;
        }

        [TestCase(ExpectedResult = true)]
        public async Task<bool> ObfuscateAndEmitSafetyKatz()
        {
            Solution safetyKatzSolution = await TestHelpers.GetSafetyKatzSolution();
            var syntaxTrees = await safetyKatzSolution.GetSyntaxTreesAsync();
            Compilation compilation = CSharpCompilation.Create("SekretKatz");

            compilation = TestHelpers.GetAssemblyArray().
                Aggregate(compilation, (current, assembly) => 
                    current.AddReferences(MetadataReference.CreateFromFile(assembly.Location)));

            compilation = compilation.AddSyntaxTrees(syntaxTrees);

            var versions = syntaxTrees.Select(tree => ((CSharpParseOptions) tree.Options).LanguageVersion).ToList();
            
            SourceObfuscator obfuscator = new SourceObfuscator();
            // compilation = obfuscator.ObfuscatePInvokeCalls(compilation);
            // compilation = obfuscator.HideLongStringLiteralsInResource(compilation);
            // compilation = obfuscator.ObfuscateStringConstants(compilation);
            // compilation = obfuscator.ObfuscateNamespaces(compilation);
            // compilation = obfuscator.ObfuscateIdentifiers(compilation);

            compilation = obfuscator.Obfuscate(compilation);

            List<string> syntaxTreeStrings = compilation.SyntaxTrees.Select(tree => tree.ToString()).ToList();

            string outputPath = TestHelpers.AssemblyDirectory + Path.DirectorySeparatorChar + "SecretKatz.exe";
            return obfuscator.EmitAssembly(compilation, outputPath); ;
        }

    }
}
