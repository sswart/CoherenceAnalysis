using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Analyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Cohesion";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            //context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);

            //context.RegisterCompilationAction(AnalyzeCompilation);

            context.RegisterSyntaxNodeAction<SyntaxKind>(AnalyzeSyntaxNode, SyntaxKind.NamespaceDeclaration);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var score = GetCoherenceReport((NamespaceDeclarationSyntax)context.Node);
            if (score.IncoherentReferenceCount > 0)
            {
                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), context.ContainingSymbol.Name, score.IncoherenceScore, score.IncoherentReferenceCount);

                context.ReportDiagnostic(diagnostic);
            }
        }

        //private void AnalyzeCompilation(CompilationAnalysisContext context)
        //{
        //    var trees = context.Compilation.SyntaxTrees;
        //    foreach(var tree in trees)
        //    {
        //        var root = tree.GetCompilationUnitRoot();
        //        GetCoherenceReport(root);
        //        var walker = new Walker();
        //        walker.Visit(root);

        //        var relevantReferences = walker.Usings.Where(str => str.StartsWith(walker.Namespace));
        //        var totalPenalty = relevantReferences
        //            .Select(r => CoherenceCalculator.GetCoherencePenalty(walker.Namespace, r))
        //            .Sum();
        //    }
        //}

        private CoherenceReport GetCoherenceReport(NamespaceDeclarationSyntax node)
        {
            var walker = new Walker();
            walker.Visit(node.Parent);

            var nodeNamespace = node.Name.ToString();
            var assemblyNamespace = ((QualifiedNameSyntax)node.Name).Left.ToString();

            var relevantReferences = walker.Usings.Where(str => str.StartsWith(assemblyNamespace));
            var coherencePenalties = relevantReferences
                .Select(r => CoherenceCalculator.GetCoherencePenalty(nodeNamespace, r));

            var totalPenalty = coherencePenalties.Sum();

            var incoherentReferenceCount = coherencePenalties.Count(penalty => penalty > 0);

            return new CoherenceReport
            {
                IncoherenceScore = totalPenalty,
                IncoherentReferenceCount = incoherentReferenceCount
            };
        }

        public class CoherenceReport
        {
            public int IncoherenceScore { get; set; }
            public int IncoherentReferenceCount { get; set; }
        }
        public class CoherenceCalculator
        {
            public static int GetCoherencePenalty(string sourceNamespace, string usingNamespace)
            {
                var biggestMatch = FindBiggestMatch(sourceNamespace, usingNamespace);

                var prunedSource = sourceNamespace.Substring(biggestMatch.Length );
                var prunedUsing = usingNamespace.Substring(biggestMatch.Length);

                if (string.IsNullOrEmpty(prunedSource) || string.IsNullOrEmpty(prunedUsing))
                {
                    return 0;
                }
                return prunedSource.Split('.').Length + prunedUsing.Split('.').Length;
            }

            public static string FindBiggestMatch(string one, string two)
            {
                for (int i = 0; i < one.Length; i++)
                {
                    if (one[i] != two[i])
                    {
                        return one.Substring(0, i);
                    }
                }
                return one;
            }
        }
        

        public class Walker : CSharpSyntaxWalker
        {
            public override void VisitUsingDirective(UsingDirectiveSyntax node)
            {
                Usings.Add(node.Name.ToString());
                base.VisitUsingDirective(node);
            }

            public readonly List<string> Usings = new List<string>();
        }
    }
}
