using System;
using System.Linq;
using System.Reflection;

namespace RoslynObfuscator.Obfuscation.InjectedClasses
{
    public static class IndirectObjectLoader
    {
        public static Type GetTypeFromString(string typeString)
        {
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

        public static object InitializeTypeWithArgs(Type type, object[] args)
        {
            ConstructorInfo constructor = type.GetConstructor(args.Select(arg => arg.GetType()).ToArray());
            if (constructor != null)
            {
                return constructor.Invoke(args);
            }
            else
            {
                throw new ArgumentException("Could not find constructor matching provided arguments array");
            }

        }

        public static object InvokeMethodOnObject(object obj, string methodName, object[] args)
        {
            MethodInfo methodInfo = obj.GetType().GetMethod(methodName, args.Select(arg => arg.GetType()).ToArray());
            if (methodInfo != null)
            {
                return methodInfo.Invoke(obj, args);
            }
            else
            {
                throw new ArgumentException("No appropriate Method matching name with arguments could be found.");
            }
        }
    }
}
