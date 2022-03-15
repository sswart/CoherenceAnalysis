using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analyzer.Test
{
    [TestClass]
    public class CoherenceCalculatorTests
    {
        [TestMethod]
        public void FindBiggestMatch()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.Two";

            var match = AnalyzerAnalyzer.CoherenceCalculator.FindBiggestMatch(one, two);
            match.Should().Be("Test.Namespace.");
        }

        [TestMethod]
        public void FindBiggestMatch_Same()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.One";

            var match = AnalyzerAnalyzer.CoherenceCalculator.FindBiggestMatch(one, two);
            match.Should().Be("Test.Namespace.One");
        }

        [TestMethod]
        public void FindBiggestMatch_NoMatch()
        {
            var one = "Test.Namespace.One";
            var two = "Some.Namespace.One";

            var match = AnalyzerAnalyzer.CoherenceCalculator.FindBiggestMatch(one, two);
            match.Should().Be("");
        }

        [TestMethod]
        public void FindCoherencePenalty()
        {
            var one = "Test.Namespace.One";
            var two = "Test.Namespace.Two";

            var match = AnalyzerAnalyzer.CoherenceCalculator.GetCoherencePenalty(one, two);
            match.Should().Be(2);
        }

        [TestMethod]
        public void FindCoherencePenalty_Far()
        {
            var one = "Test.Namespace.One";
            var two = "Some.Namespace.One";

            var match = AnalyzerAnalyzer.CoherenceCalculator.GetCoherencePenalty(one, two);
            match.Should().Be(6);
        }
    }
}
