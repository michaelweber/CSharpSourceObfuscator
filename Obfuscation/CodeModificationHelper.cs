using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RoslynObfuscator.Obfuscation
{
    public static class CodeModificationHelper
    {
        public static List<TextChange> GeneralizeIndentifierMethodInvocations(SyntaxTree tree, SyntaxToken identifier, Type generalizedType)
        {
            var affectedTokens =
                tree.GetRoot().DescendantTokens().Where(token => token.Text.Equals(identifier.Text)).ToList();

            List<SyntaxNode> affectedNodes =
                affectedTokens.Where(token => token.Parent is IdentifierNameSyntax).Select(token => token.Parent).ToList();

            List<TextChange> textChanges = new List<TextChange>();

            foreach (var affectedNode in affectedNodes)
            {
                if (affectedNode is IdentifierNameSyntax ins &&
                    ins.Parent is MemberAccessExpressionSyntax mae &&
                    mae.Parent is InvocationExpressionSyntax ies)
                {
                    var methodName = mae.Name.ToString();
                    var argumentString = ies.ArgumentList.Arguments.ToFullString();

                    string indirectInvocationFormatString =
                        "IndirectObjectLoader.InvokeMethodOnObject({0}, \"{1}\", new object[] {{{2}}})";
                    
                    string indirectInvocationString = string.Format(indirectInvocationFormatString, affectedNode.ToString(),
                        methodName, argumentString);

                    TextChange change = new TextChange(ies.Span, indirectInvocationString);
                    textChanges.Add(change);
                }
            }

            return textChanges;
        }

        public static SyntaxTree InsertClassDeclarationIntoSyntaxTree(SyntaxTree destTree, ClassDeclarationSyntax nodeToInsert)
        {
            CompilationUnitSyntax rootNode = (CompilationUnitSyntax)destTree.GetRoot();
            NamespaceDeclarationSyntax nsDeclarationSyntax = rootNode.DescendantNodes()
                .Where(node => node is NamespaceDeclarationSyntax).Cast<NamespaceDeclarationSyntax>().First();

            var modifiedTree = nsDeclarationSyntax.Members.Add(nodeToInsert as ClassDeclarationSyntax);
            
            SourceText destSourceText = destTree.GetText().WithChanges(
                new TextChange(nsDeclarationSyntax.Members.FullSpan, modifiedTree.ToFullString())
            );

            return destTree.WithChangedText(destSourceText);
        }

        public static string GetMetadataStringFromPInvokeSyntaxNode(MethodDeclarationSyntax pinvokeNode)
        {
            var dllImportAttribute = pinvokeNode.DescendantNodes().OfType<AttributeSyntax>().First(a => a.ToFullString().ToLower().StartsWith("dllimport"));
            //The first attribute argument of DllImport is the Library - remove the "s
            var libraryName = dllImportAttribute.ArgumentList.Arguments.First().Expression.ToString().Replace("\"", "");
            var otherAttributeArgs = dllImportAttribute.ArgumentList.Arguments.Skip(1).ToList();
            var entryPointSyntaxMatches = otherAttributeArgs
                .Where(a => a.NameEquals.ToString().StartsWith("EntryPoint")).ToList();
            var CharSetSyntaxMatches = otherAttributeArgs
                .Where(a => a.NameEquals.ToString().StartsWith("CharSet")).ToList();

            string entryPoint = "";
            if (entryPointSyntaxMatches.Count > 0)
            {
                entryPoint = entryPointSyntaxMatches.First().Expression.ToString().Replace("\"", "");
            }

            string charSet = "CharSet.Auto";
            if (CharSetSyntaxMatches.Count > 0)
            {
                charSet = CharSetSyntaxMatches.First().Expression.ToString().Replace("\"", "");
            }

            string functionName = pinvokeNode.Identifier.ToString();
            string returnTypeString = pinvokeNode.ReturnType.ToString();
            List<string> paramStrings = new List<string>();
            foreach (var parameter in pinvokeNode.ParameterList.Parameters)
            {
                //For now we ignore MarshalAs attributes
                var pString = parameter.WithAttributeLists(new SyntaxList<AttributeListSyntax>()).ToString();
                paramStrings.Add(pString);
            }

            string metadataFormatString = "{0}:{1}|{2}|{3}|{4}";
            string metadataName = (entryPoint.Length > 0) ? entryPoint : functionName;
            string metadataString = string.Format(metadataFormatString, libraryName, charSet, metadataName, returnTypeString,
                string.Join("|", paramStrings));

            return metadataString;
        }


        public static SyntaxTree AddImportsToSyntaxTree(SyntaxTree destTree, SyntaxTree srcTree)
        {
            CompilationUnitSyntax destSyntax = (CompilationUnitSyntax)destTree.GetRoot();
            CompilationUnitSyntax srcSyntax = (CompilationUnitSyntax)srcTree.GetRoot();

            SyntaxList<UsingDirectiveSyntax> mergedImportList = destSyntax.Usings;


            foreach (var usingSyntax in srcSyntax.Usings)
            {
                if (destSyntax.Usings.Any(us => us.Name.IsEquivalentTo(usingSyntax.Name)))
                {
                    continue;
                }

                mergedImportList = mergedImportList.Add(usingSyntax);
            }

            SourceText destSourceText = destTree.GetText().WithChanges(
                new TextChange(destSyntax.Usings.FullSpan, mergedImportList.ToFullString())
            );

            return destTree.WithChangedText(destSourceText);
        }

        public static SyntaxTree RenameSymbol(SemanticModel model, SyntaxToken token, string newName)
        {
            IEnumerable<TextSpan> renameSpans = GetRenameSpans(model, token).OrderBy(s => s);

            SourceText newSourceText = model.SyntaxTree.GetText()
                .WithChanges(renameSpans.Select(s => new TextChange(s, newName)));

            return model.SyntaxTree.WithChangedText(newSourceText);
        }

        public static IEnumerable<TextSpan> GetRenameSpans(SemanticModel model, in SyntaxToken token)
        {
            var node = token.Parent;
            ISymbol symbol = CodeIntrospectionHelper.GetSymbolForToken(model, token);

            if (symbol == null)
            { 
                return null;
            }

            var definitions =
                from location in symbol.Locations
                where location.SourceTree == node.SyntaxTree
                select location.SourceSpan;

            var usages =
                from t in model.SyntaxTree.GetRoot().DescendantTokens()
                where t.Text == symbol.Name
                let s = CodeIntrospectionHelper.GetSymbolForToken(model, t)
                where s == symbol
                select t.Span;

            var usageSymbols =
                from t in model.SyntaxTree.GetRoot().DescendantTokens()
                where t.Text == symbol.Name
                select model.GetSymbolInfo(t.Parent).Symbol;

            var generics =
                from generic in model.SyntaxTree.GetRoot().DescendantNodes().OfType<GenericNameSyntax>()
                where generic.Identifier.Text == symbol.Name
                select generic.Identifier.Span;

            if (symbol.Kind != SymbolKind.NamedType)
                return definitions.Concat(usages).Concat(generics).Distinct();

            var structors =
                from type in model.SyntaxTree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>()
                where type.Identifier.Text == symbol.Name
                let declaredSymbol = model.GetDeclaredSymbol(type)
                where declaredSymbol == symbol
                from method in type.Members
                let constructor = method as ConstructorDeclarationSyntax
                let destructor = method as DestructorDeclarationSyntax
                where constructor != null || destructor != null
                let identifier = constructor?.Identifier ?? destructor.Identifier
                select identifier.Span;



            return definitions.Concat(usages).Concat(structors).Concat(generics).Distinct();
        }

    }
}
