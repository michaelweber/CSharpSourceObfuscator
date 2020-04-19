using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace RoslynObfuscator
{
    public static class SolutionExtensions
    {
        public static async Task<List<SyntaxTree>> GetSyntaxTreesAsync(this Solution solution)
        {
            var documents = solution.Projects.First().Documents.ToList();

            var documentTreesAsync = documents.Select(async (document) => await document.GetSyntaxTreeAsync());
            var documentTrees = (await Task.WhenAll(documentTreesAsync)).ToList();

            return documentTrees;
        }
    }
}
