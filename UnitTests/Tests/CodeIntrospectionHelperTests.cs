using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using ObfuscatorUnitTests.Tests.TestCases;
using RoslynObfuscator.Obfuscation;

namespace ObfuscatorUnitTests.Tests
{
    [TestFixture]
    class CodeIntrospectionHelperTests
    {
        [Test]
        public void TestGetDeclarationIdentifiersForSyntaxTree1()
        {
            Compilation testCompilation = TestHelpers.GetRenamingTestCompilation();
            SyntaxTree tree = testCompilation.SyntaxTrees.First();

            var userDefinedTokens = CodeIntrospectionHelper.GetDeclarationIdentifiersFromTree(tree);

            foreach (var token in userDefinedTokens)
            {
                Console.WriteLine(token);
            }

            Assert.AreEqual(5, userDefinedTokens.Count);
        }

        [Test]
        public void TestGetDeclarationIdentifiersForSyntaxTree2()
        {
            Compilation testCompilation = TestHelpers.GetSharpDumpCompilation();
            SyntaxTree tree = testCompilation.SyntaxTrees.First();

            var userDefinedTokens = CodeIntrospectionHelper.GetDeclarationIdentifiersFromTree(tree);

            foreach (var token in userDefinedTokens)
            {
                Console.WriteLine(token);
            }

            Assert.AreEqual(37,userDefinedTokens.Count);

        }

        [Test]
        public void TestGetImportedIdentifiersFromTreeMultiFileTest()
        {
            Compilation multiCompilation = TestHelpers.GetMultiFileTestCompilation();
            var testTree = multiCompilation.SyntaxTrees.First();

            var importedIdentifiers = CodeIntrospectionHelper.GetIdentifierUsagesFromTree(testTree);

            Assert.IsTrue(importedIdentifiers.Any(token => token.ValueText.Equals("MultiFileTest2")));
            Assert.IsTrue(importedIdentifiers.Any(token => token.ValueText.Equals("StaticProperty")));
            Assert.IsTrue(importedIdentifiers.Any(token => token.ValueText.Equals("ConstantProperty")));
            Assert.IsTrue(importedIdentifiers.Any(token => token.ValueText.Equals("PublicMethod")));
        }
    }
}
