# CSharpSourceObfuscator
A C# Solution Source Obfuscator for avoiding AV signatures with minimal user interaction. Powered by the Roslyn C# library.

The goal of this project is to create a tool which, when provided with a path to a C# solution file, will obfuscate all of the source code and then generate a binary which is expected to bypass static analysis with minimal input from the user on an updated Windows 10 desktop. This is intended to extend the life of public C# red team tooling such as SpecterOps' GhostPack tools at https://github.com/GhostPack/. Once every payload in GhostPack can be compiled and executed on a fully patched Windows 10 machine, this project will be considered feature complete.

The obfuscation approach for this project involves a series of transforms which we can apply in order to reduce detection rates and break signatures. Many of these transforms involve injecting additional functionality into the output binary. For example, if we encrypt every string, we'll need to also inject string decryption logic so that the strings are usable at runtime. Injected classes/functionality can be found in the `Obfuscation/InjectedClasses` folder.

Transforms are done by importing the relevant Solution file, parsing each source file into a SyntaxTree using the Roslyn CodeAnalysis library, and then making changes. Currently the code mainly ives in the `SourceObfuscator.cs` class, but this will be refactored as more transforms are added.

# Transforms
These are the transforms that are supported by the obfuscator.

## Identifier Obfuscation
Identifier Obfuscation is when we transform variable names, method names, property names, class names, and other user defined names within the code base into something else. For PoC purposes, the replacement string randomly generates gibberish strings of a hard coded length - but a more final version would be better served by using a pre-populated dictionary of Software Engineering buzzwords and selecting them at random. When AV sees a variable name like `NiV3AC9CXTk2NTYl`, it is reason to be suspicious (or at least assume obfuscation has happened). Names like `ProcessFlowManager` are less likely to draw attention.

The current implementation of renaming is a little dumber than Roslyn tooling can enable. The `Renamer` class appears to not always work across files for certain cases so the obfuscator handles this by performing two renaming passes on each file. The first pass renames identifiers in each individual file. The second pass updates where those identifiers are used in other files. For example, in pass 1 `MyClass.MyClassFunction` would be renamed in `MyClass.cs`. In pass 2, usages of `MyClass` and `MyClassFunction` would be renamed in all other files in the targeted solution. Roslyn's semantic modeling is used to make sure that if we have two different classes that use the same name, ex: `Class1.Compress` and `Class2.Compress`, the `Compress` identifier is renamed to two separate strings depending on which symbols are observed via Semantic Analysis.

TL;DR Everyone starts this thinking "I could just do this with string replace" - it gets way more complicated than that very quickly and this is the #1 reason that Roslyn was used for this project.

## String Encryption
A common element that AV can sig on is string usage. For example, if a binary has the entire header of MimiKatz embedded in it in plaintext, that's very likely to be blocked. In response, malware will frequently encrypt every string it uses and then change the key in between campaigns to prevent their binaries from re-using the same static content. For PoC purposes the obfuscator a randomly generated XOR key to encrypt strings. It also injects the `StringEncryptor` class for decrypting them at run time. When applied, this transform causes:

~~~c#
string myString = "SUPERSECRET";
~~~

to become:

~~~c#
string myString = StringEncryptor.Decrypt("ABCDEF12345679==");
~~~

## Type Reflection Obfuscation
Compiled C# contains a large amount of type information which tools like `dnSpy` can easily parse to determine what classes are used. This makes it relatively easy to use static analysis to identify if a class is used or not in a binary. This information can help support AV signatures. For example, when C# is used to do direct interaction with memory, the `Marshal` class is often used. Some AV also seems to consider the use of symbols from the `System.IO.Compression` namespace such as `GZipStream` to be suspicious.

By taking advantage of C#'s Reflection libraries, these symbols can be hidden from static analysis. The Type Reflection obfuscation action injects the `IndirectObjectLoader` class, which contains helper functions allowing class constructors and their methods to be invoked purely based off of string usage. For example,

~~~c#
using (GZipStream gZipStream = new GZipStream(null, CompressionLevel.Fastest))
{
    gZipStream.Write(Payload, 0, Payload.Length);
}
~~~

becomes:

~~~c#
using (IDisposable gZipStream = (IDisposable)(IndirectObjectLoader.InitializeTypeWithArgs(IndirectObjectLoader.GetTypeFromString("System.IO.Compression.GZipStream"),new object[] {null, CompressionLevel.Fastest})))
{
    IndirectObjectLoader.InvokeMethodOnObject(gZipStream, "Write", new object[] {Payload, 0, Payload.Length});
}
~~~

Since `GZipStream` and the `Write` method are now encapsulated within a string, this can be combined with the above String Encryption obfuscation and Identifier Obfuscation to create something that looks nothing like the original code:

~~~c#
using (IDisposable E0HC06B4RB = (IDisposable)(OMTW68YV0B.O0112X0Z2T(OMTW68YV0B.R28MSFCT8C(CC8GFD6UA5.H82K6NH9HD("YjcxTSdUYBMVeXIhL0kwXD0pMzhfYAVjK0kdLigyUCM=")),new object[] {FMOZO3PLZJ, CompressionMode.Compress, false})))
{
    OMTW68YV0B.GYB6SV0I08(E0HC06B4RB, CC8GFD6UA5.H82K6NH9HD("ZjwrTSc="), new object[] {P7VQA1L0O9, 0, P7VQA1L0O9.Length});
}
~~~

## Large Payload / Embedded Resource Obfuscation
A common dropper technique for malware is to store an obfuscated version of the main payload, decrypt it at runtime, and then execute it. This is often done in memory to prevent the deobfuscated payload from ever touching disk. For example, the `SafetyKatz` project contains a base64 encoded version of a compressed Mimikatz binary which it manually loads using Win32 APIs at runtime. The payload itself is stored as a string in a `Constants.cs` file (at https://github.com/GhostPack/SafetyKatz/blob/master/SafetyKatz/Constants.cs). As a generalized mechanism for detecting this, AV ML solutions will be immediately suspicious of overly long strings. In my experience the difference between Windows Defender claiming a binary was malware or not was the length of the string - even if the string was completely garbage and there was no way to decrypt it.

The PoC used by this obfuscator is to generate a fake audio resource, embed the payload bytes into that, and then extract it at runtime. The WAV generation process, defined in `Obfuscation/Cryptography/AudioSteganography.cs` will create a WAV header matching the size needed to contain the payload, then XOR the payload bytes against a generated key, and finally insert the key and payload into the sample data. *Note: This isn't actually steganography, the generated WAV file is purely white noise, but the effort is being made to confuse automation - if an analyst examines this file the game is already over.* We inject in the `StegoResourceLoader` class to handle the retrieval and decryption of the payload at runtime.

In an attempt to confuse machine learning solutions, the WAV file, when extracted, is first passed into a `SoundPlayer` object. The stream is then worked with by accessing the `SoundPlayer.Stream` property. This effort alone is enough to prevent the majority of AV providers on VirusTotal from detecting a payload (statically). Most of this logic is stored within `Obfuscation/InjectedClasses/StegoResourceLoader.cs` - the transform changes something like:

~~~c#
// compressed mimikatz.exe output from Out-CompressedDLL
public static string compressedMimikatzString = "zL17fBNV+jg8aVJIoWUCNFC1apC...another 250kb of stuff";
~~~

into:

~~~c#
// compressed mimikatz.exe output from Out-CompressedDLL
public static string compressedMimikatzString = StegoResourceLoader.GetPayloadFromWavFile(StegoResourceLoader.GetResourceBytes("GKTRA477DW"));
~~~        

with strings encrypted and identifiers obfuscated we get:

~~~c#
// compressed mimikatz.exe output from Out-CompressedDLL
public static string DC65W90MB5 = ADAFNUIT6P.FD19TXWJYX(ADAFNUIT6P.LCTS0L3XZM(YMY82OMG8W.CO1C4GFYLD("dAZ0BgARDRt7HQ==")));
~~~

## P/Invoke Obfuscation
P/Invoke is a technology that allows you to access structs, callbacks, and functions in unmanaged libraries from your managed code. Unmanaged libraries in this case means any calls to `user32.dll`, `kernel32.dll`, or any of the binaries that expose Windows API functionality. Any security tool written in C# takes advantage of P/Invoke to access calls to read memory, create threads, dump process information, or do just about anything that requires deeper interaction with the operating system. These calls have a fairly recognizable signature:

~~~c#
[DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);
~~~

The DllImport attribute, extern modifier, or required usage of an identifier or string that matches the API name make it fairly easy to pick these functions out from a static analysis perspective. That also makes them fairly easy for AV to identify. While using these functions isn't normally a guarantee that something malicious is happening, P/Invoke references to enough "suspicious" functions will make it much more likely that a binary is tagged as "bad" by ML solutions.

To avoid these signatures, the obfuscator removes all P/Invoke references and replaces them with an invocation to the injected `PInvokeLoader` class, which uses `System.Reflection.Emit` to dynamically create a .NET assembly at runtime which defines the required P/Invoke expressions. P/Invoke statements are changed into a flat metadata string indicating all the information we'll need to load the function at runtime. Arguments are stuffed into an array and passed through to `PinvokeLoader`. If a parameter is an `out` parameter, then the generated replacement will instantiate it and assign it before returning. For example, the above `MiniDumpWriteDump` function becomes:

~~~c#
static bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam)
{
	object[] args = new object[] {hProcess, processId, hFile, dumpType, expParam, userStreamParam, callbackParam};
	var result = PInvokeLoader.Instance.InvokePInvokeFunction("dbghelp.dll:CharSet.Unicode|MiniDumpWriteDump|bool|IntPtr hProcess|uint processId|SafeHandle hFile|uint dumpType|IntPtr expParam|IntPtr userStreamParam|IntPtr callbackParam", args);
    return (bool)result;
}
~~~

or, after additional obfuscation:

~~~c#
static bool V3895DJ644(IntPtr G51VAOLFMK, uint IIS0G9S3TR, SafeHandle YYN6US2EIC, uint IBH3KSBU34, IntPtr IJUQ6DAI3H, IntPtr W6DM9ZC0LR, IntPtr RZJCBHCZZ9)
{
	object[] OK5EJC3DTO = new object[] {G51VAOLFMK, IIS0G9S3TR, YYN6US2EIC, IBH3KSBU34, IJUQ6DAI3H, W6DM9ZC0LR, RZJCBHCZZ9};
	var Z6ZLGYZ4HT =  YSTDTJB2OP.ZYQT6WCZSJ.PP3VZBX3VZ(DSCAC6BEB9.JC1E1JQLHU("MiQ3WV00NxsxWzp8E1lZKhRQIRkDKDlSVzwiSRheOC8URFUoEEc8QzMCJVxIJCVaOlsqDz5FaCw1FT1nJCkzVEsrO0A8WSJmIENXOyJGJn4yOgNQXj0PVDtTOiNwWX4xK1ApQj8oJBFcLSpFAU4mIyx4ViwXQScXMz4gYVkqJlgpfjgyAEVKeDJGMEUFMiJUWTUXVCdWOzoZX0wIM0d1VDcqPFNZOyxlNEU3Kw=="), OK5EJC3DTO);
    return (bool)Z6ZLGYZ4HT;
}
~~~
