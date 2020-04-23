# CSharpSourceObfuscator
A C# Solution Source Obfuscator for avoiding AV signatures with minimal user interaction. Powered by the Roslyn C# library.

The general approach for this project is that obfuscation can involve a series of transforms which we can apply in order to reduce detection rates and break signatures. Many of these transforms involve injecting additional functionality into the output binary. For example, if we encrypt every string, we'll need to also inject string decryption logic so that the strings are usable at runtime. Injected classes/functionality can be found in the `Obfuscation/InjectedClasses` folder.

Transforms are done by importing the relevant Solution file, parsing each source file into a SyntaxTree using the Roslyn CodeAnalysis library, and then making changes. Currently the code mainly ives in the `SourceObfuscator.cs` class, but this will be refactored as more transforms are added.

# Transforms
These are the transforms that are supported by the obfuscator.

## Identifier Obfuscation
Identifier Obfuscation is when we transform variable names, method names, property names, class names, and other user defined names within the code base into something else. For PoC purposes, the replacement string randomly generates gibberish strings of a hard coded length - but a more final version would be better served by using a pre-populated dictionary of Software Engineering buzzwords and selecting them at random. When AV sees a variable name like `NiV3AC9CXTk2NTYl`, it is reason to be suspicious (or at least assume obfuscation has happened). Names like `ProcessFlowManager` are less likely to draw attention.

The current implementation of renaming is a little dumber than Roslyn tooling can enable. The `Renamer` class appears to not always work across files for certain cases so the obfuscator handles this by performing two renaming passes on each file. The first pass renames identifiers in each individual file. The second pass updates where those identifiers are used in other files. For example, in pass 1 `MyClass.MyClassFunction` would be renamed in `MyClass.cs`. In pass 2, usages of `MyClass` and `MyClassFunction` would be renamed in all other files in the targeted solution. Roslyn's semantic modeling is used to make sure that if we have two different classes that use the same name, ex: `Class1.Compress` and `Class2.Compress`, the `Compress` identifier is renamed to two separate strings depending on which symbols are observed via Semantic Analysis.

TL;DR Everyone starts this thinking "I could just do this with string replace" - it gets way more complicated than that very quickly and this is the #1 reason that Roslyn was used for this.

## String Encryption
A common element that AV can sig on is string usage. For example, if a binary has the entire header of MimiKatz embedded in it in plaintext, that's very likely to be blocked. In response, malware will frequently encrypt every string it uses and then change the key in between campaigns to prevent their binaries from re-using the same static content. For PoC purposes the obfuscator a randomly generated XOR key to encrypt strings. It also injects the `StringEncryptor` class for decrypting them at run time. When applied, this transform causes:

~~~c#
string myString = "SUPERSECRET";
~~~

to become:

~~~c#
string myString = StringEncryptor.Decrypt("ABCDEF12345679==");
~~~
