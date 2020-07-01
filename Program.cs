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
using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.CodeAnalysis.Text;
using RoslynObfuscator.Obfuscation;

namespace RoslynObfuscator
{
    class Program
    {
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

        /// <summary>
        /// A command line tool to obfuscate C# projects for antivirus evasion.
        /// </summary>
        /// <param name="input">The input .cs or .sln file to obfuscate. Only .sln files can be compiled to assemblies.</param>
        /// <param name="outputDirectory">A directory to output each obfuscated .cs file</param>
        /// <param name="outputAssemblyFilePath">The full filename path where the compiled obfuscated solution should be emitted</param>
        /// <param name="ObfuscationWordList"> wordlist to use to obfuscate binary instead of default alphabet</param>
        /// <returns></returns>
        static int Main(FileInfo input, FileInfo outputDirectory, FileInfo outputAssemblyFilePath, string ObfuscationWordList)
        {
            if (input == null || input.Exists == false)
            {
                Console.WriteLine("--input argument must point to a valid .sln or .cs file. -? for usage instructions.");
                return 1;
            }

            bool inputIsSolution = input.Extension.Equals(".sln");
            bool inputIsCSFile = input.Extension.Equals(".cs");

            if (inputIsSolution && outputDirectory == null && outputAssemblyFilePath == null)
            {
                Console.WriteLine("Solution Files must provide either an --output-assembly-file-path or --output-directory argument");
                return 1;
            }

            if (inputIsCSFile && outputAssemblyFilePath != null)
            {
                Console.WriteLine("Cannot emit an assembly for .cs files");
                return 1;
            }

            if (input.Exists == false)
            {
                Console.WriteLine("File {0} does not exist.", input.FullName);
                return 1;
            }
            SourceObfuscator obfuscator = new SourceObfuscator() ; 
            if (ObfuscationWordList != null)
            {
                //Wordlist helped against heuristics/ML sometimes
                Console.WriteLine("Using wordlist {0}", ObfuscationWordList);
                PolymorphicCodeOptions options = new PolymorphicCodeOptions(RandomStringMethod.StringFromWordlistFile, 10, 20, "notused", ObfuscationWordList);
                obfuscator = new SourceObfuscator(options);
            }
            //If we're given a single file
            if (input.Extension.Equals(".cs"))
            {
                Compilation c = CSharpCompilation.Create(input.Name);
                string fileText = File.ReadAllText(input.FullName);
                SyntaxTree fileTree = CSharpSyntaxTree.ParseText(fileText);

                c = c.AddSyntaxTrees(fileTree);

                fileTree = obfuscator.Obfuscate(fileTree, c);

                if (outputDirectory != null && outputDirectory.Exists)
                {
                    File.WriteAllText(outputDirectory.FullName + Path.DirectorySeparatorChar + input.Name, fileTree.ToString());
                }
                else
                {
                    Console.WriteLine(fileTree);
                }
                return 0;
            }
            //If we're given a solution
            if (input.Extension.Equals(".sln"))
            {
                string solutionPath = input.FullName;

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

                    Console.WriteLine($"Loading solution '{solutionPath}'");

                    
                    // Attach progress reporter so we print projects as they are loaded.
                    var solution = workspace.OpenSolutionAsync(solutionPath, new ConsoleProgressReporter()).Result;

                    Console.WriteLine($"Finished loading solution '{solutionPath}'");

                    string assemblyName = Path.GetFileNameWithoutExtension(input.FullName);

                    if (outputAssemblyFilePath != null)
                    {
                        assemblyName = Path.GetFileNameWithoutExtension(outputAssemblyFilePath.FullName);
                    }

                    Compilation compilation = CSharpCompilation.Create(assemblyName);

                    //TODO Add in the references from whatever solution is being used
                    //BUG Fix identifiers from not correctly obtaining static symbols when same assembly is added multiple times
                    //var solutionReferences = solution.Projects.SelectMany(p => p.MetadataReferences).ToList();
                    // compilation = compilation.AddReferences(solutionReferences);

                    var injectedReferences = GetAssemblyArray()
                        .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

                    compilation = compilation.AddReferences(injectedReferences);
                    var syntaxTrees = solution.GetSyntaxTreesAsync().Result;
                    compilation = compilation.AddSyntaxTrees(syntaxTrees);

                    var versions = syntaxTrees.Select(tree => ((CSharpParseOptions)tree.Options).LanguageVersion).ToList();

                    compilation = obfuscator.Obfuscate(compilation);

                    if (outputDirectory != null)
                    {
                        int unnamedFileCounter = 1;
                        foreach (var syntaxTree in compilation.SyntaxTrees)
                        {
                            string fileName = Path.GetFileName(syntaxTree.FilePath);
                            if (string.IsNullOrEmpty(fileName))
                            {
                                fileName = assemblyName + unnamedFileCounter + ".cs";
                                unnamedFileCounter += 1;
                            }

                            if (outputDirectory.Exists == false)
                            {
                                try
                                {
                                    Directory.CreateDirectory(outputDirectory.FullName);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("{0} does not exist and could not be created.", outputDirectory);
                                    return 1;
                                }
                            }

                            File.WriteAllText(outputDirectory.FullName + Path.DirectorySeparatorChar + fileName, syntaxTree.ToString());
                        }
                    }

                    if (outputAssemblyFilePath != null)
                    {
                        obfuscator.EmitAssembly(compilation, outputAssemblyFilePath.FullName); ;
                    }

                    return 0;
                }
            }

            return 1;
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
