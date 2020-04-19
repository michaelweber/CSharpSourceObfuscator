using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Rename;
using NUnit.Framework;
using RoslynObfuscator.Obfuscation;
using RoslynObfuscator.Tests.TestCases;

namespace RoslynObfuscator.Tests
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


            compilation = obfuscator.ObfuscateIdentifiers(compilation);

            SyntaxTree tree = compilation.SyntaxTrees.First();

            string treeString = tree.ToString();
            Console.WriteLine(treeString);
            
            Assert.IsFalse(treeString.Contains("XorBytes"));
            

            return true;

        }
    }
}
