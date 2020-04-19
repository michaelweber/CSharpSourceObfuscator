using System;

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using RoslynObfuscator.Obfuscation;

namespace RoslynObfuscator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Attempt to set the version of MSBuild.
            var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            var instance = visualStudioInstances.Length == 1
                // If there is only one instance of MSBuild on this machine, set that as the one to use.
                ? visualStudioInstances[0]
                // Handle selecting the version of MSBuild you want to use.
                : SelectVisualStudioInstance(visualStudioInstances);

            Console.WriteLine($"Using MSBuild at '{instance.MSBuildPath}' to load projects.");

            // NOTE: Be sure to register an instance with the MSBuildLocator 
            //       before calling MSBuildWorkspace.Create()
            //       otherwise, MSBuildWorkspace won't MEF compose.
            MSBuildLocator.RegisterInstance(instance);

            using (var workspace = MSBuildWorkspace.Create())
            {
                // Print message for WorkspaceFailed event to help diagnosing project load failures.
                workspace.WorkspaceFailed += (o, e) => Console.WriteLine(e.Diagnostic.Message);

                // var solutionPath = args[0];
                var solutionPath =
                    @"S:\projects\malware-dropper\SharpDump-master\Original\SharpDump-master\SharpDump.sln";
                Console.WriteLine($"Loading solution '{solutionPath}'");

                // Attach progress reporter so we print projects as they are loaded.
                var solution = await workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter());


                Console.WriteLine($"Finished loading solution '{solutionPath}'");

                var documents = solution.Projects.SelectMany(p => p.Documents).ToList();

                var sharpDumpDoc = documents[0];

                var compilation = await sharpDumpDoc.Project.GetCompilationAsync();

                var tree = await sharpDumpDoc.GetSyntaxTreeAsync();

                var model = compilation.GetSemanticModel(tree);
                var tokens = tree.GetRoot().DescendantTokens();

                var allSymbols =
                    (from t in model.SyntaxTree.GetRoot().DescendantTokens()
                    select new Tuple<SyntaxToken,ISymbol>(t, 
                        model.GetSymbolInfo(t.Parent).Symbol ?? model.GetDeclaredSymbol(t.Parent))).ToList();


                var stringLiterals = model.SyntaxTree.GetRoot().DescendantTokens().Where(
                    token => token.Kind() == SyntaxKind.StringLiteralToken).ToList();

                SyntaxToken compress = tokens.First(t => t.Text == "Compress");

                ISymbol compressSymbol = model.GetSymbolInfo(compress.Parent).Symbol ?? model.GetDeclaredSymbol(compress.Parent);

                var references = await SymbolFinder.FindReferencesAsync(compressSymbol, solution);

                SyntaxTree outputTree = CodeModificationHelper.RenameSymbol(model, compress, "SCRAMBLED");
                Console.WriteLine(outputTree);

                // TODO: Do analysis on the projects in the loaded solution 
            }
        }


        private static VisualStudioInstance SelectVisualStudioInstance(VisualStudioInstance[] visualStudioInstances)
        {
            Console.WriteLine("Multiple installs of MSBuild detected please select one:");
            for (int i = 0; i < visualStudioInstances.Length; i++)
            {
                Console.WriteLine($"Instance {i + 1}");
                Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
                Console.WriteLine($"    Version: {visualStudioInstances[i].Version}");
                Console.WriteLine($"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
            }

            while (true)
            {
                var userResponse = Console.ReadLine();
                if (int.TryParse(userResponse, out int instanceNumber) &&
                    instanceNumber > 0 &&
                    instanceNumber <= visualStudioInstances.Length)
                {
                    return visualStudioInstances[instanceNumber - 1];
                }
                Console.WriteLine("Input not accepted, try again.");
            }
        }

        private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
        {
            public void Report(ProjectLoadProgress loadProgress)
            {
                var projectDisplay = Path.GetFileName(loadProgress.FilePath);
                if (loadProgress.TargetFramework != null)
                {
                    projectDisplay += $" ({loadProgress.TargetFramework})";
                }

                Console.WriteLine($"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
            }
        }
    }
}
