using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

namespace Analyzer.Test
{
    public class CohesionCalculatorTests
    {
        [Fact]
        public void FindBiggestMatch()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.Two";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.FindBiggestMatch(one, two);
            match.Should().Be("Test.Namespace.");
        }

        [Fact]
        public void FindBiggestMatch_Same()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.One";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.FindBiggestMatch(one, two);
            match.Should().Be("Test.Namespace.One");
        }

        [Fact]
        public void FindBiggestMatch_NoMatch()
        {
            var one = "Test.Namespace.One";
            var two = "Some.Namespace.One";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.FindBiggestMatch(one, two);
            match.Should().Be("");
        }

        [Fact]
        public void FindCohesionPenalty()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.Two";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.GetCohesionPenalty(one, two);
            match.Should().Be(2);
        }

        [Fact]
        public void FindCohesionPenalty_Far()
        {
            var one = "Test.Namespace.One";
            var two = "Some.Namespace.One";

            var match = AnalyzerAnalyzer.ExternalCohesionCalculator.GetCohesionPenalty(one, two);
            match.Should().Be(6);
        }
    }
}
