using FluentAssertions;
using Umbraco.Commerce.PaymentProviders;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Nets.UnitTests
{
    public class NetsOrderReconciliationTests
    {
        // Issue #838: product 414375 + shipping 210500 - discount 62488 = 562387,
        // but the order transaction amount rounds to 562388, a difference of +1.
        [Fact]
        public void CalculateRoundingAdjustment_When_Items_Undershoot_Order_Amount_Returns_Positive_Difference()
        {
            NetsOrderItem[] items =
            [
                new NetsOrderItem { GrossTotalAmount = 414375 },
                new NetsOrderItem { GrossTotalAmount = 210500 },
                new NetsOrderItem { GrossTotalAmount = -62488 },
            ];

            long adjustment = NetsOrderReconciliation.CalculateRoundingAdjustment(562388, items);

            adjustment.Should().Be(1);
            (items.Sum(x => (long)x.GrossTotalAmount) + adjustment).Should().Be(562388);
        }

        [Fact]
        public void CalculateRoundingAdjustment_When_Items_Overshoot_Order_Amount_Returns_Negative_Difference()
        {
            NetsOrderItem[] items =
            [
                new NetsOrderItem { GrossTotalAmount = 100 },
                new NetsOrderItem { GrossTotalAmount = 100 },
            ];

            long adjustment = NetsOrderReconciliation.CalculateRoundingAdjustment(199, items);

            adjustment.Should().Be(-1);
        }

        [Fact]
        public void CalculateRoundingAdjustment_When_Totals_Match_Returns_Zero()
        {
            NetsOrderItem[] items =
            [
                new NetsOrderItem { GrossTotalAmount = 500 },
                new NetsOrderItem { GrossTotalAmount = 250 },
            ];

            long adjustment = NetsOrderReconciliation.CalculateRoundingAdjustment(750, items);

            adjustment.Should().Be(0);
        }
    }
}
