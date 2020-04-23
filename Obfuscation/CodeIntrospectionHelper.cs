using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RoslynObfuscator.Obfuscation
{
    public static class CodeIntrospectionHelper
    {
        public static List<NameSyntax> GetUserNamespacesFromTree(SyntaxTree tree)
        {
            var descendants = tree.GetRoot().DescendantNodes();
            var namespaceDeclarationNodes = descendants.Where(node => node.IsKind(SyntaxKind.NamespaceDeclaration)).Cast<NamespaceDeclarationSyntax>().ToList();
            var namespaceIdentifiers = namespaceDeclarationNodes.Select(nd =>
                nd.Name).ToList();

            return namespaceIdentifiers;
        }

        public static List<SyntaxToken> GetDeclarationIdentifiersFromTree(SyntaxTree tree)
        {
            List<SyntaxToken> userDefinedIdentifiers = new List<SyntaxToken>();

            var descendants = tree.GetRoot().DescendantNodes();

            var methodDeclarationNodes = descendants.Where(node => node.IsKind(SyntaxKind.MethodDeclaration)).Cast<MethodDeclarationSyntax>().ToList();
            var classDeclarationNodes= descendants.Where(node => node.IsKind(SyntaxKind.ClassDeclaration)).Cast<ClassDeclarationSyntax>().ToList();
            var variableDeclarationNodes = descendants.Where(node => node.IsKind(SyntaxKind.VariableDeclaration)).Cast<VariableDeclarationSyntax>().ToList();
            var propertyDeclarationNodes= descendants.Where(node => node.IsKind(SyntaxKind.PropertyDeclaration)).Cast<PropertyDeclarationSyntax>().ToList();
            var parameterNodes = descendants.Where(node => node.IsKind(SyntaxKind.Parameter)).Cast<ParameterSyntax>().ToList();

            var methodIdentifiers = methodDeclarationNodes.Select(md => md.Identifier).ToList();
            var classIdentifiers = classDeclarationNodes.Select(cd => cd.Identifier).ToList();
            var variableIdentifiers = variableDeclarationNodes.SelectMany(vd => vd.Variables.Select(v => v.Identifier).ToList()).ToList();
            var propertyIdentifiers = propertyDeclarationNodes.Select(pdn => pdn.Identifier).ToList();
            var parameterIdentifiers = parameterNodes.Select(pn => pn.Identifier).ToList();

            return userDefinedIdentifiers.Concat(methodIdentifiers).
                Concat(classIdentifiers).Concat(variableIdentifiers).Concat(propertyIdentifiers).Concat(parameterIdentifiers).ToList();
        }

        public static List<SyntaxToken> GetIdentifierUsagesFromTree(SyntaxTree tree)
        {
            var descendants = tree.GetRoot().DescendantNodes();

            //Parent Syntax Targets:
            //MemberAccessExpressionSyntax
            //ObjectCreationExpressionSyntax

            var identifiers =
                (from descendant in descendants
                where descendant.IsKind(SyntaxKind.IdentifierName) &&
                      (descendant.Parent.IsKind(SyntaxKind.ObjectCreationExpression) ||
                       descendant.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression) ||
                       descendant.Parent.IsKind(SyntaxKind.InvocationExpression))
                select ((IdentifierNameSyntax) descendant).Identifier).ToList();

            return identifiers;
        }

        public static IEnumerable<ISymbol> GetAllSymbols(Compilation compilation, SyntaxNode root)
        {
            var noDuplicates = new HashSet<ISymbol>();

            var model = compilation.GetSemanticModel(root.SyntaxTree);

            foreach (var node in root.DescendantNodesAndSelf())
            {
                ISymbol symbol = model.GetDeclaredSymbol(node) ??
                                 model.GetSymbolInfo(node).Symbol;

                if (symbol != null)
                {
                    if (noDuplicates.Add(symbol))
                        yield return symbol;
                }
            }
        }

        public static ISymbol GetSymbolForToken(SemanticModel model, SyntaxToken token)
        {
            var node = token.Parent;
            ISymbol symbol = model.GetSymbolInfo(node).Symbol ?? model.GetDeclaredSymbol(node);

            //There's an edge case with identifiers where they reference a static method - this should catch some of those
            if (symbol == null && node.IsKind(SyntaxKind.IdentifierName))
            {
                List<ISymbol> staticMatches = model.LookupStaticMembers(token.SpanStart).
                    Where(member => member.IsStatic && member.Name.Equals(token.ValueText)).ToList();

                if (staticMatches.Count == 1)
                {
                    symbol = staticMatches.First();
                }
                else if (staticMatches.Count > 1)
                {
                    Console.WriteLine("Multiple static matches detected...which one do we pick?");
                    throw new NotImplementedException();
                }
                else
                {
                    //This isn't a static edge case
                    return null;
                }
            }

            return symbol;
        }

        public static ClassDeclarationSyntax GetFirstClassDeclarationFromSyntaxTree(SyntaxTree tree)
        {
            return tree.GetRoot().DescendantNodes()
                .Where(node => node.IsKind(SyntaxKind.ClassDeclaration)).Cast<ClassDeclarationSyntax>().First();
        }

    }
}
