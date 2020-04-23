using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace RoslynObfuscator.Obfuscation.InjectedClasses
{
    /// <summary>
    /// Allows dynamic creation of P/Invoke DLLImports at Runtime.
    /// Code adapted from https://stackoverflow.com/questions/44578167/dynamically-invoke-unmanaged-code-from-c-sharp
    /// </summary>
    public class PInvokeLoader
    {
        private static PInvokeLoader _instance = null;

        private readonly ModuleBuilder _module;
        private Dictionary<string, Type> _cachedPInvokeTypes = new Dictionary<string, Type>();

        public static PInvokeLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PInvokeLoader();
                }

                return _instance;
            }
        }

        protected PInvokeLoader()
        {
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("Win32"), AssemblyBuilderAccess.Run);
            _module = assembly.DefineDynamicModule("Win32", emitSymbolInfo: false);

            _cachedPInvokeTypes = new Dictionary<string, Type>();
        }

        private Type GetTypeFromString(string typeString)
        {
            if (typeString.Equals("IntPtr")) return typeof(IntPtr);
            if (typeString.Equals("UIntPtr")) return typeof(UIntPtr);
            if (typeString.Equals("Int16") || typeString.Equals("short")) return typeof(short);
            if (typeString.Equals("UInt16") || typeString.Equals("ushort")) return typeof(UInt16);
            if (typeString.Equals("Int32") || typeString.Equals("int")) return typeof(int);
            if (typeString.Equals("UInt32") || typeString.Equals("uint")) return typeof(UInt32);
            if (typeString.Equals("Int64") || typeString.Equals("long")) return typeof(long);
            if (typeString.Equals("UInt64") || typeString.Equals("ulong")) return typeof(UInt64);
            if (typeString.ToLower().Equals("string")) return typeof(string);
            if (typeString.ToLower().Equals("char")) return typeof(char);
            if (typeString.ToLower().Equals("byte")) return typeof(byte);
            if (typeString.ToLower().Equals("sbyte")) return typeof(sbyte);
            if (typeString.ToLower().Equals("double")) return typeof(double);
            if (typeString.ToLower().Equals("decimal")) return typeof(decimal);
            if (typeString.Equals("bool") || typeString.Equals("Boolean")) return typeof(bool);
            if (typeString.Equals("void")) return typeof(void);

            foreach (Assembly b in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = b.GetType(typeString);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private Type BuildPInvokeFromMetadata(string functionName, string library, CharSet charSet, Type returnType, Type[] paramTypes, string[] argsMetadata)
        {
            var typeBuilder = _module.DefineType(functionName + "Class", TypeAttributes.Class | TypeAttributes.Public);

            var dllImportCtor = typeof(DllImportAttribute).GetConstructor(new Type[] { typeof(string) });
            //This is where we can jam other attributes that have been parsed out
            var dllImportBuilder = new CustomAttributeBuilder(dllImportCtor, new object[] { library });

            //Anything we're going to use for a ref/out parameter needs to have the Type converted to a RefType
            IEnumerable<Type> updatedParameters = new List<Type>();
            for (int parameterIndex = 1; parameterIndex <= argsMetadata.Length; parameterIndex += 1)
            {

                string argMetadata = argsMetadata[parameterIndex - 1];
                if (argMetadata.StartsWith("ref ") || argMetadata.StartsWith("out "))
                {
                    updatedParameters = updatedParameters.Append(paramTypes[parameterIndex - 1].MakeByRefType());
                }
                else
                {
                    updatedParameters = updatedParameters.Append(paramTypes[parameterIndex - 1]);
                }
            }

            var pinvokeBuilder = typeBuilder.DefinePInvokeMethod(
                name: functionName,
                dllName: library,
                entryName: functionName,
                attributes: MethodAttributes.Static | MethodAttributes.Public,
                callingConvention: CallingConventions.Standard,
                returnType: returnType,  // typeof(void) if there is no return value.
                parameterTypes: updatedParameters.ToArray(),
                nativeCallConv: CallingConvention.Winapi,
                nativeCharSet: charSet);

            /*
             //Might be needed for MarshalAs PInvoke expressions
            for (int parameterIndex = 1; parameterIndex <= argsMetadata.Length; parameterIndex += 1)
            {
                string argMetadata = argsMetadata[parameterIndex - 1];
                ParameterAttributes attributes = ParameterAttributes.None;
                if (argMetadata.StartsWith("ref"))
                {
                    attributes = attributes | ParameterAttributes.In | ParameterAttributes.Out;
                }
                else if (argMetadata.StartsWith("out"))
                {
                    attributes = attributes | ParameterAttributes.Out;
                }
                else
                {
                    attributes = attributes | ParameterAttributes.In;
                }

                string paramName = argMetadata.Split(' ').Last();

                pinvokeBuilder.DefineParameter(parameterIndex, attributes, paramName);
            }*/

            pinvokeBuilder.SetCustomAttribute(dllImportBuilder);

            Type pinvokeType = typeBuilder.CreateType();
            return pinvokeType;
        }
        public object InvokePInvokeFunction(string invokeMetadata, params object[] args)
        {
            //invokeMetadata is a string in the form of 
            //LibraryName|FunctionName|ReturnType|Arg1Type|Arg2Type|Arg3Type...

            string[] parameters = invokeMetadata.Split('|');

            string libName = parameters[0].Split(':')[0];
            CharSet charSet;
            if (parameters[0].Split(':').Length > 1)
            {
                string charSetString = parameters[0].Split(':')[1];
                bool charSetParsed = CharSet.TryParse(charSetString.Split('.')[1], out charSet);
            }
            else
            {
                charSet = CharSet.Auto;
            }
            
            string functionName = parameters[1];
            Type returnType = GetTypeFromString(parameters[2]);
            Type[] argTypes = args.Select(arg => arg.GetType()).ToArray();
            string[] argMetadata = parameters.Skip(3).ToArray(); 

            Type invocationType;

            if (_cachedPInvokeTypes.ContainsKey(invokeMetadata))
            {
                invocationType = _cachedPInvokeTypes[invokeMetadata];
            }
            else
            {
                invocationType = BuildPInvokeFromMetadata(functionName, libName, charSet, returnType, argTypes, argMetadata);
                _cachedPInvokeTypes.Add(invokeMetadata, invocationType);
            }

            var method = invocationType.GetMethod(functionName, BindingFlags.Static | BindingFlags.Public);
            var result = method.Invoke(null, args);

            return result;
        }

    }
}
