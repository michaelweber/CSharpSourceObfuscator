using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoslynObfuscator.Obfuscation.InjectedClasses
{
    public enum InjectableClasses
    {
        StringEncryptor,
        IndirectObjectLoader,
        StegoResourceLoader,
        Properties,
        PInvokeLoader
    }
    public static class InjectedClassHelper
    {
        public static string GetInjectableClassSourceText(InjectableClasses injectableClass)
        {
            string resourceName = $"RoslynObfuscator.Obfuscation.InjectedClasses.{injectableClass}.cs";
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
