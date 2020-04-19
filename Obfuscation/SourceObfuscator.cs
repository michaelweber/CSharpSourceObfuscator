﻿using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using RoslynObfuscator.Obfuscation.InjectedClasses;


namespace RoslynObfuscator.Obfuscation
{
    public class SourceObfuscator
    {
        private Dictionary<NameSyntax, string> renamedNamespaces;
        private Dictionary<string, string> renamedMembers;

        //HACK: Currently we just replace everything with the same Namespace since we're going 
        //      to rename EVERYTHING which should avoid collisions and we don't know if a file
        //      has implicitly imported a different file by using the same namespace
        private string ObfuscatedNamespace;

        private PolymorphicCodeOptions polymorphicCodeOptions;

		public SourceObfuscator(PolymorphicCodeOptions codeOptions = null)
        {
            polymorphicCodeOptions = codeOptions;

            if (codeOptions == null)
            {
                polymorphicCodeOptions = PolymorphicCodeOptions.Default;
            }

            //Generate the encryption key we'll use for this set of obfuscation
            string encryptionKey = PolymorphicGenerator.GetRandomString(polymorphicCodeOptions);
            StringEncryptor.Key = encryptionKey;

            ObfuscatedNamespace = PolymorphicGenerator.GetRandomIdentifier(polymorphicCodeOptions);

            renamedMembers = new Dictionary<string, string>();
            renamedNamespaces = new Dictionary<NameSyntax, string>();
        }

        private string GetSymbolTokenLookupKey(SyntaxToken token, ISymbol symbol)
        {
            if (symbol == null)
            {
                string guessedLookupName = this.renamedMembers.Keys.Where(key => key.EndsWith(token.ValueText)).FirstOrDefault();
                if (guessedLookupName == null)
                {
                    return "UNKNOWNSYMBOL::" + token.ValueText;
                }
                return guessedLookupName;
            }
            else
            {
                return symbol.ToDisplayString() + "::" + token.ValueText;
            }
        }

        private string GetNewNameForNamespace(NameSyntax nameSyntax)
        {
            if (renamedNamespaces.ContainsKey(nameSyntax))
            {
                return renamedNamespaces[nameSyntax];
            }

            //Just use the same namespace for everything for now
            string newNamespaceName = ObfuscatedNamespace;
            renamedNamespaces.Add(nameSyntax, newNamespaceName);
            return newNamespaceName;
        }
        private string GetNewNameForTokenAndSymbol(SyntaxToken token, ISymbol symbol, bool renameUnseenIdentifiers = true)
        {
            Dictionary<string, string> relevantDictionary = null;

            string symbolTokenLookupKey = GetSymbolTokenLookupKey(token, symbol);

            if (token.Parent.IsKind(SyntaxKind.PropertyDeclaration) ||
                token.Parent.IsKind(SyntaxKind.MethodDeclaration) ||
                token.Parent.IsKind(SyntaxKind.VariableDeclarator) ||
                token.Parent.IsKind(SyntaxKind.ClassDeclaration) ||
                token.Parent.IsKind(SyntaxKind.Parameter) ||
                token.Parent.IsKind(SyntaxKind.IdentifierName) && token.Parent.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                relevantDictionary = renamedMembers;
            }

            //Don't mess with external functions
            if (symbol != null && symbol.IsExtern)
            {
                return token.ValueText;
            }

            if (relevantDictionary != null)
            {
                if (relevantDictionary.ContainsKey(symbolTokenLookupKey))
                {
                    return relevantDictionary[symbolTokenLookupKey];
                }
                string newName = PolymorphicGenerator.GetRandomIdentifier(polymorphicCodeOptions);
                relevantDictionary.Add(symbolTokenLookupKey, newName);
                return newName;
            }
            
            if (renamedMembers.ContainsKey(symbolTokenLookupKey))
            {
                return renamedMembers[symbolTokenLookupKey];
            }

            if (renameUnseenIdentifiers)
            {
                //If this is just some random variable name that won't be accessed by another file
                //we don't need to track the change
                return PolymorphicGenerator.GetRandomIdentifier(polymorphicCodeOptions);
            }
            else
            {
                //If we're not renaming unseen identifiers, then just return the original value
                return token.ValueText;
            }
        }


        public SyntaxTree ObfuscateTypeReferences(SyntaxTree syntaxTree, Compilation compilation, List<Type> typesToObfuscate)
        {
            List<TextChange> changes = new List<TextChange>();

            foreach (Type type in typesToObfuscate)
            {
                var matchingTokens =
                    syntaxTree.GetRoot().DescendantTokens().Where(token => token.Text.Contains(type.Name)).ToList();


                var affectedNodes =
                    matchingTokens.Select(token => token.Parent).ToList();

                SemanticModel model = compilation.GetSemanticModel(syntaxTree);

                var affectedSymbols = affectedNodes.Select(node => model.GetSymbolInfo(node)).ToList();

                List<SyntaxNode> nodesToObfuscate = new List<SyntaxNode>();

                for (int index = 0; index < affectedNodes.Count; index += 1)
                {
                    if (affectedSymbols[index].Symbol != null &&
                        affectedSymbols[index].Symbol.ToDisplayString().Equals(type.FullName))
                    {
                        nodesToObfuscate.Add(affectedNodes[index]);
                    }
                }


                List<SyntaxNode> parents = nodesToObfuscate.Select(node => node.Parent).Distinct().ToList();

                //If we define something that implements IDisposable, make sure it's typed as that instead in case
                //we're used inside a Using statement
                bool typeIsDisposable = type.GetInterfaces().Contains(typeof(IDisposable));

                List<SyntaxToken> affectedVariables = new List<SyntaxToken>();

                //Apply changes to the following cases
                //TargetType variable = new TargetType(constructorArgs);
                foreach (var parent in parents)
                {
                    
                    //var variable = new TargetType(constructorArgs);
                    if (parent is VariableDeclarationSyntax vd)
                    {
                        string replacementTypeString = "var";

                        if (typeIsDisposable)
                        {
                            replacementTypeString = "IDisposable";
                        }

                        //The variable name used here might be used to access functions - we need to 
                        //generalize how those are accessed as well
                        SyntaxToken varNameSyntaxToken = vd.Variables.First().Identifier;
                        affectedVariables.Add(varNameSyntaxToken);

                        // Console.WriteLine("Replacing {0} with {1}", vd.Type.ToString(), replacementTypeString);
                        changes.Add(new TextChange(vd.Type.Span, replacementTypeString));
                    }
                    //TargetType variable = IDOL.InitializeTypeWithArgs(IDOL.GetTypeFromString("System.IO.Compression.GZipStream"), new object[] {constructorArgs});
                    else if (parent is ObjectCreationExpressionSyntax oce)
                    {
                        string indirectLoadFormatString =
                            "IndirectObjectLoader.InitializeTypeWithArgs(IndirectObjectLoader.GetTypeFromString(\"{0}\"),new object[] {{{1}}})";

                        if (typeIsDisposable)
                        {
                            indirectLoadFormatString = "(IDisposable)(" + indirectLoadFormatString + ")";
                        }

                        string indirectLoadString = string.Format(indirectLoadFormatString, type.FullName,
                            oce.ArgumentList.Arguments.ToFullString());
                        // Console.WriteLine("Replacing {0} with {1}", oce.ToString(), indirectLoadString);
                        changes.Add(new TextChange(oce.Span, indirectLoadString));
                    }
                }

                foreach (var affectedVariable in affectedVariables)
                {
                    List<TextChange> variableChanges =
                        CodeModificationHelper.GeneralizeIndentifierMethodInvocations(syntaxTree, affectedVariable,
                            type);
                    changes = changes.Concat(variableChanges).ToList();
                }
            }

            SourceText newSourceText = syntaxTree.GetText();
            newSourceText = newSourceText.WithChanges(changes);
            SyntaxTree newTree = syntaxTree.WithChangedText(newSourceText);

            string iolSourceText = InjectedClassHelper.GetInjectableClassSourceText(InjectedClassHelper
                .InjectableClasses
                .IndirectObjectLoader);
            SyntaxTree iolTree = CSharpSyntaxTree.ParseText(iolSourceText);
            ClassDeclarationSyntax iolClass = CodeIntrospectionHelper.GetFirstClassDeclarationFromSyntaxTree(iolTree);

            newTree = CodeModificationHelper.InsertClassDeclarationIntoSyntaxTree(newTree, iolClass);
            newTree = CodeModificationHelper.AddImportsToSyntaxTree(newTree, iolTree);

            return newTree;
        }

        public Compilation ObfuscateStringConstants(Compilation compilation)
        {
            List<SyntaxTree> oldTrees = compilation.SyntaxTrees.ToList();

            bool injectedEncryptor = false;

            foreach (SyntaxTree tree in oldTrees)
            {
                //We can obfuscate AssemblyInfo.cs later
                if (tree.FilePath.Contains("AssemblyInfo.cs"))
                {
                    continue;
                }

                SyntaxTree oldTree = tree;

                SyntaxTree newTree = ObfuscateStringConstants(tree, !injectedEncryptor);

                //After the first tree is obfuscated, we inject in the string encryptor
                injectedEncryptor = true;

                compilation = compilation.ReplaceSyntaxTree(oldTree, newTree);
            }

            return compilation;
        }


        private SyntaxTree InjectStringEncryptor(SyntaxTree syntaxTree)
        {
            string encryptorSourceText = InjectedClassHelper.GetInjectableClassSourceText(InjectedClassHelper
                .InjectableClasses
                .StringEncryptor);

            encryptorSourceText = encryptorSourceText.Replace("RANDOMIZEME", StringEncryptor.Key);

            SyntaxTree encryptorTree = CSharpSyntaxTree.ParseText(encryptorSourceText);

            ClassDeclarationSyntax encryptorClass =
                CodeIntrospectionHelper.GetFirstClassDeclarationFromSyntaxTree(encryptorTree);

            SyntaxTree modifiedTree = CodeModificationHelper.InsertClassDeclarationIntoSyntaxTree(syntaxTree, encryptorClass);
            modifiedTree = CodeModificationHelper.AddImportsToSyntaxTree(modifiedTree, encryptorTree);

            return modifiedTree;
        }

        public SyntaxTree ObfuscateStringConstants(SyntaxTree syntaxTree, bool injectStringEncryptor = true)
        {
            string replacementFormatString = "StringEncryptor.DecryptString(\"{0}\")";

            List<TextChange> stringEncryptorChanges =
                (from token in syntaxTree.GetRoot().DescendantTokens()
                where token.IsKind(SyntaxKind.StringLiteralToken) && token.Parent.Parent.Kind() != SyntaxKind.AttributeArgument
                select new TextChange(token.Span,
                    string.Format(replacementFormatString, StringEncryptor.EncryptString(token.ValueText)))).ToList();

            SourceText newSourceText = syntaxTree.GetText()
                .WithChanges(stringEncryptorChanges);

            SyntaxTree treeWithEncryptedStrings = syntaxTree.WithChangedText(newSourceText);

            if (injectStringEncryptor)
            {
                treeWithEncryptedStrings = InjectStringEncryptor(treeWithEncryptedStrings);
            }

            return treeWithEncryptedStrings;
        }

        public Compilation ObfuscateIdentifiers(Compilation compilation)
        {
            List<SyntaxTree> trees = compilation.SyntaxTrees.ToList();
            Dictionary<SyntaxTree, List<TextChange>> treeChanges = new Dictionary<SyntaxTree, List<TextChange>>();
            //First replace all the identifiers in each file
            foreach (SyntaxTree tree in trees)
            {
                List<TextChange> changes = new List<TextChange>();
                List<SyntaxToken> userIdentifiers = CodeIntrospectionHelper.GetDeclarationIdentifiersFromTree(tree);
                changes = ObfuscateIdentifiers(tree, compilation, userIdentifiers);
                treeChanges.Add(tree, changes);
            }

            //Then we do a second pass and replace references to classes/properties/fields
            //in other files that have changed in pass 1
            foreach (SyntaxTree tree in trees)
            {
                List<TextChange> changes = treeChanges[tree];

                List<SyntaxToken> identifiersToPossiblyChange = 
                    CodeIntrospectionHelper.GetIdentifierUsagesFromTree(tree);

                changes = changes.Concat(
                    ObfuscateIdentifiers(tree, compilation, identifiersToPossiblyChange, false)
                    ).ToList();
                changes = changes.Distinct().ToList();

                treeChanges[tree] = changes;
            }

            //Apply the source changes from pass 1 + pass 2
            foreach (SyntaxTree tree in trees)
            {
                SyntaxTree oldTree = tree;
                
                List<TextChange> changes = treeChanges[tree];

                try
                {
                    SourceText changedText = tree.GetText().WithChanges(changes);
                    SyntaxTree newTree = tree.WithChangedText(changedText);
                    newTree = ObfuscateNamespaces(newTree);
                    compilation = compilation.ReplaceSyntaxTree(oldTree, newTree);
                }
                catch (ArgumentException arg)
                {
                    changes = changes.OrderBy(s => s.Span.Start).ToList();
                    List<TextChange> overlaps = changes.Where(c =>
                        changes.Any(cc =>
                            ((c.Span.Start >= cc.Span.Start && c.Span.Start <= cc.Span.End) ||
                             (c.Span.End <= cc.Span.End && c.Span.End >= cc.Span.End)) && cc != c)).ToList();

                    Console.WriteLine(overlaps);
                }
            }

            return compilation;
        }

        public SyntaxTree ObfuscateIdentifiers(SyntaxTree tree, Compilation compilation)
        {
            SyntaxTree returnTree = tree;
            List<SyntaxToken> userIdentifiers = CodeIntrospectionHelper.GetDeclarationIdentifiersFromTree(tree);
            List<TextChange> changes = ObfuscateIdentifiers(tree, compilation, userIdentifiers);
            SourceText changedText = returnTree.GetText().WithChanges(changes);
            returnTree = returnTree.WithChangedText(changedText);
            returnTree = ObfuscateNamespaces(returnTree);
            return returnTree;
        }

        private List<TextChange> ObfuscateIdentifiers(SyntaxTree tree, Compilation compilation, 
            List<SyntaxToken> identifiers, bool renameUnseenIdentifiers = true)
        {
            SemanticModel model = compilation.GetSemanticModel(tree);
            List<TextChange> changes = new List<TextChange>();
            foreach (var identifier in identifiers)
            {
                //Don't rename our entry point function
                if (identifier.Text.Equals("Main") &&
                    identifier.Parent.IsKind(SyntaxKind.MethodDeclaration))
                {
                    continue;
                }

                ISymbol symbol = CodeIntrospectionHelper.GetSymbolForToken(model, identifier);

                  IEnumerable<TextSpan> renameSpans = CodeModificationHelper.GetRenameSpans(model, identifier);

                if (renameSpans == null)
                {
                    //Happens when we encounter Static classes that aren't defined in projects,
                    //ex: Encoding.UTF8.GetString - the Encoding identifier matches search critera but
                    //doesn't match any symbols
                    continue;
                }

                renameSpans = renameSpans.OrderBy(s => s);
                string newName = GetNewNameForTokenAndSymbol(identifier, symbol, renameUnseenIdentifiers);


                changes = changes.Concat(renameSpans.Select(s => new TextChange(s, newName))).ToList();
            }

            changes = changes.Distinct().ToList();
            return changes;
        }

        private SyntaxTree ObfuscateNamespaces(SyntaxTree tree)
        {
            List<NameSyntax> namespaceNodes = CodeIntrospectionHelper.GetUserNamespacesFromTree(tree);

            List<TextChange> changes = new List<TextChange>();
            foreach (NameSyntax namespaceNode in namespaceNodes)
            {
                string newName = GetNewNameForNamespace(namespaceNode);
                changes.Add(new TextChange(namespaceNode.Span, newName));
            }

            SourceText newSourceText = tree.GetText().WithChanges(changes);
                
            SyntaxTree treeWithRandomizedNamespaces = tree.WithChangedText(newSourceText);

            return treeWithRandomizedNamespaces;
        }

        public SyntaxTree Obfuscate(SyntaxTree syntaxTree, Compilation compilation)
        {
            SourceText stage1, stage2, stage3;

            SyntaxTree oldTree = syntaxTree;

            syntaxTree = ObfuscateTypeReferences(syntaxTree,  compilation, new List<Type>() {typeof(GZipStream)});
            stage1 = syntaxTree.GetText();
            compilation = compilation.ReplaceSyntaxTree(oldTree, syntaxTree);
            oldTree = syntaxTree;
            syntaxTree = ObfuscateStringConstants(syntaxTree);
            compilation = compilation.ReplaceSyntaxTree(oldTree, syntaxTree);
            stage2 = syntaxTree.GetText();
            syntaxTree = ObfuscateIdentifiers(syntaxTree, compilation);

            stage3 = syntaxTree.GetText();

            return syntaxTree;
        }




    }
}