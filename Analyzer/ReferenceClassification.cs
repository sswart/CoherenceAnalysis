using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyzer
{
    public class ReferenceClassifier
    {
        public static ReferenceMetadata Classify(IEnumerable<string> referencedClasses, string nodeNamespace)
        {
            var references = referencedClasses.Select(r => Classify(r, nodeNamespace)).Where(r => r != null).Cast<ReferenceClassification>();
            return new ReferenceMetadata(nodeNamespace, references);
        }

        private static ReferenceClassification? Classify(string referenceClassName, string sourceClassName)
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

            return new ReferenceClassification(referenceClassName, direction, distance);
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

        public record ReferenceMetadata(string SourceNamespace, IEnumerable<ReferenceClassification> References);
        public record ReferenceClassification(string FullyQualifiedClassName, Direction Direction, int Distance);
        public enum Direction
        {
            StraightUp,
            Down,
            Side,
            Diagonal
        }
    }
}
