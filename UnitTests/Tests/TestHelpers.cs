using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using RoslynObfuscator.Obfuscation.InjectedClasses;

namespace ObfuscatorUnitTests.Tests.TestCases
{
    public static class TestHelpers
    {
        public static readonly string TestPath = Path.DirectorySeparatorChar + "Tests" + Path.DirectorySeparatorChar + 
                                                 "TestCases" + Path.DirectorySeparatorChar;

        public static readonly string ImagePath = Path.DirectorySeparatorChar + "img" + Path.DirectorySeparatorChar;
                                                 

        private static MSBuildWorkspace _workspace;

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public static Compilation GetUsingReplaceTestCompilation()
        {
            string path = AssemblyDirectory + TestPath + "UsingReplaceTestCase.cs";
            string unobfuscatedProgramText = File.ReadAllText(path);

            Compilation compilation = CSharpCompilation.Create("usingReplace");

            SyntaxTree tree = CSharpSyntaxTree.ParseText(unobfuscatedProgramText);
            compilation = GetAssemblyArray().Aggregate(compilation, (current, assembly) => current.AddReferences(MetadataReference.CreateFromFile(assembly.Location)));
            compilation = compilation.AddSyntaxTrees(tree);
            return compilation;
        }

        public static Compilation GetRenamingTestCompilation()
        {
            string path = AssemblyDirectory + TestPath + "AggressiveRenamingTestCase.cs";
            string unobfuscatedProgramText = File.ReadAllText(path);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(unobfuscatedProgramText);

            Assembly[] assemblies = GetAssemblyArray();

            var compilation = CSharpCompilation.Create("obfuscated");

            foreach (Assembly assembly in assemblies)
            {
                compilation = compilation.AddReferences(MetadataReference.CreateFromFile(assembly.Location));
            }

            compilation = compilation.AddSyntaxTrees(tree);

            return compilation;
        }

        public static Assembly[] GetAssemblyArray()
        {
            Assembly[] assemblies = new Assembly[]
            {
                typeof(int).Assembly,
                typeof(System.Collections.ArrayList).Assembly,
                typeof(System.Data.MappingType).Assembly,
                typeof(System.Net.Http.HttpClient).Assembly,
                typeof(System.IO.Compression.CompressionMode).Assembly,
                typeof(System.Xml.DtdProcessing).Assembly,
                typeof(System.Xml.Linq.ReaderOptions).Assembly,
                typeof(System.Media.SoundPlayer).Assembly,
                typeof(System.Linq.Enumerable).Assembly,
                typeof(System.Reflection.Assembly).Assembly,
                typeof(System.Drawing.Bitmap).Assembly,
                typeof(System.Text.Encoding).Assembly
            };

            return assemblies;
        }

        public static Compilation GetIndirectObjectLoaderCompilation()
        {
            string indirectObjectLoaderCode = InjectedClassHelper.GetInjectableClassSourceText(InjectableClasses.IndirectObjectLoader);
            SyntaxTree iojSyntaxTree = CSharpSyntaxTree.ParseText(indirectObjectLoaderCode);

            var compilation = CSharpCompilation.Create("indirectObjectLoader");
            compilation = GetAssemblyArray().Aggregate(compilation, (current, assembly) => current.AddReferences(MetadataReference.CreateFromFile(assembly.Location)));
            compilation = compilation.AddSyntaxTrees(iojSyntaxTree);
            return compilation;
        }

        public static Compilation GetStringEncryptorCompilation()
        {
            string stringEncryptorCode = InjectedClassHelper.GetInjectableClassSourceText(InjectableClasses.StringEncryptor); ;
            SyntaxTree seSyntaxTree = CSharpSyntaxTree.ParseText(stringEncryptorCode);

            var compilation = CSharpCompilation.Create("stringEncryptor");
            compilation = GetAssemblyArray().Aggregate(compilation, (current, assembly) => current.AddReferences(MetadataReference.CreateFromFile(assembly.Location)));
            compilation = compilation.AddSyntaxTrees(seSyntaxTree);
            return compilation;
        }

        public static Compilation GetMultiFileTestCompilation()
        {
            string path1 = AssemblyDirectory + TestPath + "MultiFileTest1.cs";
            string unobfuscatedProgramText1 = File.ReadAllText(path1);
            SyntaxTree syntaxTree1 = CSharpSyntaxTree.ParseText(unobfuscatedProgramText1);

            string path2 = AssemblyDirectory + TestPath + "MultiFileTest2.cs";
            string unobfuscatedProgramText2 = File.ReadAllText(path2);
            SyntaxTree syntaxTree2 = CSharpSyntaxTree.ParseText(unobfuscatedProgramText2);

            var compilation = CSharpCompilation.Create("multiFileCompilation");
            compilation = GetAssemblyArray().Aggregate(compilation, (current, assembly) => current.AddReferences(MetadataReference.CreateFromFile(assembly.Location)));
            compilation = compilation.AddSyntaxTrees(syntaxTree1, syntaxTree2);
            return compilation;
        }

        public static Compilation GetLongBadStringTestCompilation()
        {
            string path = AssemblyDirectory + TestPath + "LongBadStringTestCase.cs";
            string programText = File.ReadAllText(path);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);

            Assembly[] assemblies = GetAssemblyArray();

            var compilation = CSharpCompilation.Create("obfuscated");

            compilation = assemblies.Aggregate(compilation, (current, assembly) => current.AddReferences(MetadataReference.CreateFromFile(assembly.Location)));

            compilation = compilation.AddSyntaxTrees(tree);

            return compilation;
        }

        public static Image GetStegoImage()
        {
            string path = AssemblyDirectory + ImagePath + "vtenterprise.bmp";
            return Image.FromFile(path);
        }

        public static Image GetStegoPNG()
        {
            string path = AssemblyDirectory + ImagePath + "chrome.png";
            return Image.FromFile(path);
        }


        public static Compilation GetSharpDumpCompilation()
        {
            string unobfuscatedProgramText = File.ReadAllText(GhostPackSolutionPaths.SharpDumpSrcPath);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(unobfuscatedProgramText);

            Assembly[] assemblies = GetAssemblyArray();

            var compilation = CSharpCompilation.Create("obfuscated");

            compilation = assemblies.Aggregate(compilation, (current, assembly) => current.AddReferences(MetadataReference.CreateFromFile(assembly.Location)));

            compilation = compilation.AddSyntaxTrees(tree);

            return compilation;
        }

        public static async Task<Solution> GetSafetyKatzSolution()
        {
            if (_workspace == null)
            {
                var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
                var instance = visualStudioInstances[0];
                MSBuildLocator.RegisterInstance(instance);
                _workspace = MSBuildWorkspace.Create();
            }

            var workspace = _workspace;

            var solution = await workspace.OpenSolutionAsync(GhostPackSolutionPaths.SafetyKatzSlnPath);
            return solution;
        }


        public static Compilation GetSeatbeltCompilation()
        {
            string unobfuscatedProgramText = File.ReadAllText(GhostPackSolutionPaths.SeatbeltSrcPath);

            SyntaxTree tree = CSharpSyntaxTree.ParseText(unobfuscatedProgramText);

            Assembly[] assemblies = GetAssemblyArray();

            var compilation = CSharpCompilation.Create("obfuscated");

            compilation = assemblies.Aggregate(compilation, (current, assembly) => current.AddReferences(MetadataReference.CreateFromFile(assembly.Location)));

            compilation = compilation.AddSyntaxTrees(tree);

            return compilation;
        }

        public static async Task<Solution> GetSeatbeltSolution()
        {
            if (_workspace == null)
            {
                var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
                var instance = visualStudioInstances[0];
                MSBuildLocator.RegisterInstance(instance);
                _workspace = MSBuildWorkspace.Create();
            }

            var workspace = _workspace;

            var solution = await workspace.OpenSolutionAsync(GhostPackSolutionPaths.SeatbeltSlnPath);
            return solution;
        }
    }
}
