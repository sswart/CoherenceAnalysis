using System;
using System.Collections.Generic;
using System.Linq;

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

            var distance = CountSteps(prunedReference) + CountSteps(prunedSource);
            var direction = GetDirection(prunedSource, prunedReference);

            return new UsingClassification(referenceClassName, direction, distance);
        }

        private static Direction GetDirection(string prunedSource, string prunedReference)
        {
            if (CountSteps(prunedReference) == 0 && CountSteps(prunedSource) != 0)
            {
                return Direction.StraightUp;
            }
            else if (CountSteps(prunedReference) != 0 && CountSteps(prunedSource) == 0)
            {
                return Direction.Down;
            }
            else if (CountSteps(prunedReference) != 0 && CountSteps(prunedSource) != 0)
            {
                return Direction.Diagonal;
            }
            else
            {
                return Direction.Side;
            }
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
            Diagonal
        }
    }
}
