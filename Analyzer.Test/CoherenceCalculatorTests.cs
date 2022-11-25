using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Analyzer.Test
{
    [TestClass]
    public class CohesionCalculatorTests
    {
        [TestMethod]
        public void FindBiggestMatch()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.Two";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.FindBiggestMatch(one, two);
            match.Should().Be("Test.Namespace.");
        }

        [TestMethod]
        public void FindBiggestMatch_Same()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.One";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.FindBiggestMatch(one, two);
            match.Should().Be("Test.Namespace.One");
        }

        [TestMethod]
        public void FindBiggestMatch_NoMatch()
        {
            var one = "Test.Namespace.One";
            var two = "Some.Namespace.One";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.FindBiggestMatch(one, two);
            match.Should().Be("");
        }

        [TestMethod]
        public void FindCohesionPenalty()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.Two";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.GetCohesionPenalty(one, two);
            match.Should().Be(2);
        }

        [TestMethod]
        public void FindCohesionPenalty_Far()
        {
            var one = "Test.Namespace.One";
            var two = "Some.Namespace.One";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.GetCohesionPenalty(one, two);
            match.Should().Be(6);
        }
    }
}
