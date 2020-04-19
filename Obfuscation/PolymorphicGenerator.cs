using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace RoslynObfuscator.Obfuscation
{ 
    public enum RandomStringMethod
    {
        StringFromFixedDictionary
    }

    public sealed class PolymorphicCodeOptions
    {
        public static PolymorphicCodeOptions Default { get; } = new PolymorphicCodeOptions();

        public RandomStringMethod RandomStringMethod;

        public string CustomStringDictionary;

        public int MinimumRandomStringLength;
        public int MaximumRandomStringLength;

        public PolymorphicCodeOptions(
            RandomStringMethod method = RandomStringMethod.StringFromFixedDictionary,
            int minimumRandomStringLength = 10,
            int maximumRandomStringLength = 10,
            string customStringDictionary = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            RandomStringMethod = method;
            MaximumRandomStringLength = maximumRandomStringLength;
            MinimumRandomStringLength = minimumRandomStringLength;
            CustomStringDictionary = customStringDictionary;
        }


    }

    public static class PolymorphicGenerator
    {
        private static Random random = new Random();

        public static string GetRandomIdentifier()
        {
            return GetRandomIdentifier(PolymorphicCodeOptions.Default);
        }

        public static string GetRandomIdentifier(PolymorphicCodeOptions options)
        {

            string randomIdentifier;


            for (int attempt = 0; attempt < 100; attempt += 1)
            {
                switch (options.RandomStringMethod)
                {
                    case RandomStringMethod.StringFromFixedDictionary:
                        randomIdentifier = BuildRandomStringFromDictionary(options);
                        break;
                    default:
                        throw new ArgumentException("RandomStringMethod {0} is Not Supported", options.RandomStringMethod.ToString());
                }

                //Make sure we're generating a valid identifier
                if (Microsoft.CodeAnalysis.CSharp.SyntaxFacts.IsValidIdentifier(randomIdentifier))
                {
                    return randomIdentifier;
                }
            }


            throw new ArgumentException("Dictionary provided did not generate a valid identifier after 100 attempts.");
        }

        public static string GetRandomString()
        {
                return GetRandomString(PolymorphicCodeOptions.Default);
        }

        public static string GetRandomString(PolymorphicCodeOptions options)
        {

            switch (options.RandomStringMethod)
            {
                case RandomStringMethod.StringFromFixedDictionary:
                    return BuildRandomStringFromDictionary(options);
                default:
                    throw new ArgumentException("RandomStringMethod {0} is Not Supported", options.RandomStringMethod.ToString());
            }
        }

        private static string BuildRandomStringFromDictionary(PolymorphicCodeOptions options)
        {

            int length = random.Next(options.MinimumRandomStringLength,
                options.MaximumRandomStringLength);
            string dictionary = options.CustomStringDictionary;

            return new string(Enumerable.Repeat(dictionary, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

        }

    }
}
