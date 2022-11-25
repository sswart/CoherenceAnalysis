using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Analyzer.Test
{
    public class ReferenceClassificationTest
    {
        const string customerDetailsPhoneNumber = "Customer.Details.PhoneNumber";
        const string customerOrder = "Customer.Order";
        const string customerInfoPreferences = "Customer.Info.Preferences";
        private readonly string[] _references = new string[]
        {
            customerDetailsPhoneNumber,
            customerOrder,
            customerInfoPreferences,
            "System.Text.Json"
        };

        [Fact]
        public void Classify_OnlyThisAssembly()
        {
            var node = "Customer.Details.Address";

            var result = ReferenceClassification.Classify(new string[] {"System.Text.Json"}, node).References;

            result.Should().BeEmpty();
        }

        [Fact]
        public void Classify_Up()
        {
            var node = "Customer.Details.Address";
            
            var result = ReferenceClassification.Classify(_references, node).References.Single(r => r.FullyQualifiedClassName == customerOrder);

            result.Direction.Should().Be(ReferenceClassification.Direction.StraightUp);
            result.Distance.Should().Be(1);
        }

        [Fact]
        public void Classify_Down()
        {
            var node = customerOrder;
            var reference = "Customer.Details.Address";

            var result = ReferenceClassification.Classify(new string[] { reference }, node).References.Single();

            result.Direction.Should().Be(ReferenceClassification.Direction.Down);
            result.Distance.Should().Be(1);
        }

        [Fact]
        public void Classify_Side()
        {
            var node = "Customer.Details.Address";

            var result = ReferenceClassification.Classify(_references, node).References.Single(r => r.FullyQualifiedClassName == customerDetailsPhoneNumber);

            result.Direction.Should().Be(ReferenceClassification.Direction.Side);
            result.Distance.Should().Be(0);
        }

        [Fact]
        public void Classify_Diagonal()
        {
            var node = "Customer.Details.Address";
            var result = ReferenceClassification.Classify(_references, node).References.Single(r => r.FullyQualifiedClassName == customerInfoPreferences);
            result.Direction.Should().Be(ReferenceClassification.Direction.DiagonalUp);
            result.Distance.Should().Be(2);
        }
    }
}
