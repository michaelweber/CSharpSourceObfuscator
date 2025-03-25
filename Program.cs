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
using System.Collections.Immutable;
using System.Threading;

namespace RoslynObfuscator
{
    class Program
    {
        static Program()
        {
            try
            {
                // Try to find Visual Studio installation
                string[] possibleVsVersions = new[] { "2022", "2019", "2017" };
                string[] possibleVsEditions = new[] { "Enterprise", "Professional", "Community", "BuildTools" };
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string msbuildPath = null;

                foreach (var version in possibleVsVersions)
                {
                    foreach (var edition in possibleVsEditions)
                    {
                        var path = Path.Combine(programFiles, "Microsoft Visual Studio", version, edition, "MSBuild", "Current", "Bin");
                        if (Directory.Exists(path))
                        {
                            msbuildPath = path;
                            break;
                        }
                    }
                    if (msbuildPath != null) break;
                }

                if (msbuildPath == null)
                {
                    // Try fallback to VS2019 BuildTools specific path
                    msbuildPath = Path.Combine(programFiles, @"Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin");
                }

                if (Directory.Exists(msbuildPath))
                {
                    Console.WriteLine($"Found MSBuild at: {msbuildPath}");
                    
                    // Set environment variables
                    Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", Path.Combine(msbuildPath, "MSBuild.exe"));
                    var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                    Environment.SetEnvironmentVariable("PATH", currentPath + ";" + msbuildPath);
                    
                    // Set additional MSBuild environment variables
                    string vsInstallRoot = Path.GetFullPath(Path.Combine(msbuildPath, "..", "..", ".."));
                    Environment.SetEnvironmentVariable("VSINSTALLDIR", vsInstallRoot);
                    Environment.SetEnvironmentVariable("VisualStudioVersion", "16.0");
                    Environment.SetEnvironmentVariable("VSCMD_VER", "16.0");
                    
                    // Set .NET Framework path
                    string frameworkPath = Path.Combine(programFiles, "Reference Assemblies", "Microsoft", "Framework", ".NETFramework");
                    if (Directory.Exists(frameworkPath))
                    {
                        Environment.SetEnvironmentVariable("FrameworkSDKRoot", frameworkPath);
                    }

                    // Print diagnostic information
                    Console.WriteLine("Environment Configuration:");
                    Console.WriteLine($"  MSBUILD_EXE_PATH: {Environment.GetEnvironmentVariable("MSBUILD_EXE_PATH")}");
                    Console.WriteLine($"  VSINSTALLDIR: {Environment.GetEnvironmentVariable("VSINSTALLDIR")}");
                    Console.WriteLine($"  FrameworkSDKRoot: {Environment.GetEnvironmentVariable("FrameworkSDKRoot")}");
                }
                else
                {
                    Console.WriteLine("Warning: Could not find MSBuild installation path");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not set MSBuild environment: {ex.Message}");
            }
        }

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
        /// <param name="input">The input .cs, .csproj, or .sln file to obfuscate. Only .csproj and .sln files can be compiled to assemblies.</param>
        /// <param name="outputDirectory">A directory to output each obfuscated .cs file</param>
        /// <param name="outputAssemblyFilePath">The full filename path where the compiled obfuscated solution should be emitted</param>
        /// <param name="ObfuscationWordList">wordlist to use to obfuscate binary instead of default alphabet</param>
        /// <returns></returns>
        static int Main(FileInfo input, FileInfo outputDirectory, FileInfo outputAssemblyFilePath, string ObfuscationWordList)
        {
            try
            {
                if (input == null || input.Exists == false)
                {
                    Console.WriteLine("--input argument must point to a valid .sln, .csproj, or .cs file. -? for usage instructions.");
                    return 1;
                }

                // Normalize the input path
                string fullPath = Path.GetFullPath(input.FullName);
                input = new FileInfo(fullPath);

                bool inputIsSolution = input.Extension.Equals(".sln", StringComparison.OrdinalIgnoreCase);
                bool inputIsCSFile = input.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase);
                bool inputIsProject = input.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase);

                if ((inputIsSolution || inputIsProject) && outputDirectory == null && outputAssemblyFilePath == null)
                {
                    Console.WriteLine("Solution and Project Files must provide either an --output-assembly-file-path or --output-directory argument");
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

                SourceObfuscator obfuscator = new SourceObfuscator();
                if (ObfuscationWordList != null)
                {
                    //Wordlist helped against heuristics/ML sometimes
                    Console.WriteLine("Using wordlist {0}", ObfuscationWordList);
                    PolymorphicCodeOptions options = new PolymorphicCodeOptions(RandomStringMethod.StringFromWordlistFile, 10, 20, "notused", ObfuscationWordList);
                    obfuscator = new SourceObfuscator(options);
                }

                //If we're given a single file
                if (inputIsCSFile)
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

                //If we're given a solution or project
                if (inputIsSolution || inputIsProject)
                {
                    string path = input.FullName;
                    Console.WriteLine($"Processing path: {path}");
                    Console.WriteLine($"Absolute path: {Path.GetFullPath(path)}");

                    // Read and parse project file
                    Console.WriteLine("\nProject file contents:");
                    string projectContent = File.ReadAllText(path);
                    Console.WriteLine(projectContent);

                    var doc = new System.Xml.XmlDocument();
                    doc.LoadXml(projectContent);

                    // Get project properties
                    var assemblyName = Path.GetFileNameWithoutExtension(input.FullName);
                    if (outputAssemblyFilePath != null)
                    {
                        assemblyName = Path.GetFileNameWithoutExtension(outputAssemblyFilePath.FullName);
                    }

                    // Create compilation options
                    var compilationOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication)
                        .WithOptimizationLevel(OptimizationLevel.Release)
                        .WithPlatform(Platform.X86);

                    // Create initial compilation
                    var compilation = CSharpCompilation.Create(assemblyName, options: compilationOptions);

                    // Add basic framework references
                    var references = new List<MetadataReference>();
                    var frameworkPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                        @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5");

                    // Add references from project file
                    Console.WriteLine("\nAdding references:");
                    foreach (System.Xml.XmlNode node in doc.SelectNodes("//Reference"))
                    {
                        var include = node.Attributes?["Include"]?.Value;
                        if (!string.IsNullOrEmpty(include))
                        {
                            Console.WriteLine($"Adding reference: {include}");
                            var refPath = Path.Combine(frameworkPath, include + ".dll");
                            if (File.Exists(refPath))
                            {
                                references.Add(MetadataReference.CreateFromFile(refPath));
                            }
                        }
                    }

                    // Add our additional references
                    var injectedReferences = GetAssemblyArray()
                        .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                        .Where(reference => !references.Contains(reference));
                    references.AddRange(injectedReferences);

                    compilation = (CSharpCompilation)compilation.AddReferences(references);

                    // Find and add source files
                    Console.WriteLine("\nAdding source files:");
                    var projectDir = Path.GetDirectoryName(path);
                    var sourceFiles = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories);

                    foreach (var sourceFile in sourceFiles)
                    {
                        if (sourceFile.Contains("\\obj\\") || sourceFile.Contains("\\bin\\"))
                            continue;

                        Console.WriteLine($"Adding source file: {sourceFile}");
                        var sourceText = File.ReadAllText(sourceFile);
                        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, path: sourceFile);
                        compilation = (CSharpCompilation)compilation.AddSyntaxTrees(syntaxTree);
                    }

                    // Obfuscate the compilation
                    compilation = (CSharpCompilation)obfuscator.Obfuscate(compilation);

                    // Output files if requested
                    if (outputDirectory != null)
                    {
                        try
                        {
                            if (!outputDirectory.Exists)
                            {
                                Directory.CreateDirectory(outputDirectory.FullName);
                            }

                            int unnamedFileCounter = 1;
                            foreach (var syntaxTree in compilation.SyntaxTrees)
                            {
                                string fileName = Path.GetFileName(syntaxTree.FilePath);
                                if (string.IsNullOrEmpty(fileName))
                                {
                                    fileName = assemblyName + unnamedFileCounter + ".cs";
                                    unnamedFileCounter += 1;
                                }

                                File.WriteAllText(Path.Combine(outputDirectory.FullName, fileName), syntaxTree.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to write output files: {ex.Message}");
                            return 1;
                        }
                    }

                    // Emit assembly if requested
                    if (outputAssemblyFilePath != null)
                    {
                        obfuscator.EmitAssembly(compilation, outputAssemblyFilePath.FullName);
                    }

                    return 0;
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                return 1;
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
