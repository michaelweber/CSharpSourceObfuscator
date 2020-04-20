using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using ObfuscatorUnitTests.Tests.TestCases;
using RoslynObfuscator.Obfuscation;

namespace ObfuscatorUnitTests.Tests
{
    [TestFixture]
    class CodeModificationHelperTests
    {
        [Test]
        public void TestMergingImports()
        {
            Compilation indirectObjectLoaderCompilation = TestHelpers.GetIndirectObjectLoaderCompilation();
            Compilation stringEncryptorCompilation = TestHelpers.GetStringEncryptorCompilation();

            SyntaxTree iojTree = indirectObjectLoaderCompilation.SyntaxTrees.First();
            SyntaxTree seTree = stringEncryptorCompilation.SyntaxTrees.First();

            SyntaxTree joinedTree = CodeModificationHelper.AddImportsToSyntaxTree(seTree, iojTree);

            Console.WriteLine(joinedTree);

            int importCount = ((CompilationUnitSyntax) joinedTree.GetRoot()).Usings.Count;

            Assert.AreEqual(5, importCount);
        }

        [Test]
        public void TestAddingClassToCompilation()
        {
            Compilation indirectObjectLoaderCompilation = TestHelpers.GetIndirectObjectLoaderCompilation();
            Compilation stringEncryptorCompilation = TestHelpers.GetStringEncryptorCompilation();

            SyntaxTree iojTree = indirectObjectLoaderCompilation.SyntaxTrees.First();
            SyntaxTree seTree = stringEncryptorCompilation.SyntaxTrees.First();

            ClassDeclarationSyntax iojClassNode = iojTree.GetRoot().DescendantNodes()
                .Where(node => node.IsKind(SyntaxKind.ClassDeclaration)).Cast<ClassDeclarationSyntax>().First();

            SyntaxTree joinedTree = CodeModificationHelper.InsertClassDeclarationIntoSyntaxTree(seTree, iojClassNode);

            int numClassesInTree = joinedTree.GetRoot().DescendantNodes()
                .Count(node => node.IsKind(SyntaxKind.ClassDeclaration));

            Console.WriteLine(joinedTree);

            Assert.AreEqual(2, numClassesInTree);
            Assert.IsTrue(joinedTree.ToString().Contains("class IndirectObjectLoader"));
            Assert.IsTrue(joinedTree.ToString().Contains("class StringEncryptor"));
        }
    }
}
