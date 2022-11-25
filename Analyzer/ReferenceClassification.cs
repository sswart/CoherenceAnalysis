using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Analyzer.AnalyzerAnalyzer;

namespace Analyzer
{
    public class ReferenceClassification
    {
        public static ReferenceMetadata Classify(IEnumerable<string> relevantReferences, string nodeNamespace)
        {
            var references = relevantReferences.Select(r => Classify(r, nodeNamespace)).Where(r => r != null).Cast<UsingClassification>();
            return new ReferenceMetadata(nodeNamespace, references);
        }

        private static UsingClassification? Classify(string referenceClassName, string sourceClassName)
        {
            var match = ExternalCohesionCalculator.FindBiggestMatch(referenceClassName, sourceClassName);
            if (string.IsNullOrEmpty(match))
            {
                return null;
            }
            var prunedSource = sourceClassName.Substring(match.Length);
            var prunedReference = referenceClassName.Substring(match.Length);

            Direction direction;
            int distance;
            if (CountSteps(prunedReference) == 0 && CountSteps(prunedSource) != 0)
            {
                direction = Direction.StraightUp;
                distance = CountSteps(prunedSource);
            }
            else if (CountSteps(prunedReference) == 0 && CountSteps(prunedSource) == 0)
            {
                direction = Direction.Side;
                distance = 0;
            }
            else if (CountSteps(prunedReference) != 0 && CountSteps(prunedSource) == 0)
            {
                direction = Direction.Down;
                distance = CountSteps(prunedReference);
            }
            else if (CountSteps(prunedReference) != 0 && CountSteps(prunedSource) != 0)
            {
                direction = Direction.DiagonalUp;
                distance = CountSteps(prunedReference) + CountSteps(prunedSource);
            }
            else
            {
                direction = Direction.Side;
                distance = 0;
            }

            return new UsingClassification(referenceClassName, direction, distance);
        }

        private static int CountSteps(string prunedSource)
        {
            return prunedSource.Count(c => c == '.');
        }

        public record ReferenceMetadata(string SourceNamespace, IEnumerable<UsingClassification> References);
        public record UsingClassification(string FullyQualifiedClassName, Direction Direction, int Distance);
        public enum Direction
        {
            StraightUp,
            Down,
            Side,
            DiagonalUp
        }
    }
}
