using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class AnalyzerAnalyzer : DiagnosticAnalyzer
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

            context.RegisterSyntaxNodeAction<SyntaxKind>(AnalyzeSyntaxNode, SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction<SyntaxKind>(AnalyzeClassNode, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClassNode(SyntaxNodeAnalysisContext context)
        {
            var theClass = (ClassDeclarationSyntax)context.Node;
            var children = theClass.DescendantNodes();

            var a = DateTime.FromBinary(1);
            var b = DateTime.Now;
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            var score = GetCohesionReport((NamespaceDeclarationSyntax)context.Node);
            if (score.IncohesiveReferenceCount > 0)
            {
                var diagnostic = Diagnostic.Create(Rule, context.Node.GetLocation(), context.ContainingSymbol.Name, score.IncohesionScore, score.IncohesiveReferenceCount);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private CohesionReport GetCohesionReport(NamespaceDeclarationSyntax node)
        {
            var walker = new Walker();
            walker.Visit(node.Parent);

            var nodeNamespace = node.Name.ToString();
            var assemblyNamespace = ((QualifiedNameSyntax)node.Name).Left.ToString();

            var relevantReferences = walker.Usings.Where(str => str.StartsWith(assemblyNamespace));

            var classified = ReferenceClassification.Classify(relevantReferences, nodeNamespace);


            var coherencePenalties = relevantReferences
                .Select(r => ExternalCohesionCalculator.GetCohesionPenalty(nodeNamespace, r));

            var totalPenalty = coherencePenalties.Sum();

            var incoherentReferenceCount = coherencePenalties.Count(penalty => penalty > 0);

            return new CohesionReport
            {
                IncohesionScore = totalPenalty,
                IncohesiveReferenceCount = incoherentReferenceCount
            };
        }

        
        public class CohesionReport
        {
            public int IncohesionScore { get; set; }
            public int IncohesiveReferenceCount { get; set; }
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
        
        public class ReferenceWalker : CSharpSyntaxWalker
        {
            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                //if (node is SimpleMemberAccessExpressionSyntax expr)
                if (node.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    // check the left side of it
                }
                base.VisitMemberAccessExpression(node);
            }

            public override void VisitParameter(ParameterSyntax node)
            {
                // if it is defined in this assembly
                base.VisitParameter(node);
            }
        }

    }
}
