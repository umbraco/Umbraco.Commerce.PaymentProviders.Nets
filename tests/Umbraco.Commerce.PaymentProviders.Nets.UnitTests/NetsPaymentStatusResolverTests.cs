using FluentAssertions;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.PaymentProviders;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Nets.UnitTests
{
    public class NetsPaymentStatusResolverTests
    {
        [Fact]
        public void Resolve_When_Refund_Is_Less_Than_Charge_Returns_PartiallyRefunded()
        {
            var summary = new NetsSummary { ChargedAmount = 10000, RefundedAmount = 2500 };

            NetsPaymentStatusResolver.Resolve(summary).Should().Be(PaymentStatus.PartiallyRefunded);
        }

        [Fact]
        public void Resolve_When_Refund_Equals_Charge_Returns_Refunded()
        {
            var summary = new NetsSummary { ChargedAmount = 10000, RefundedAmount = 10000 };

            NetsPaymentStatusResolver.Resolve(summary).Should().Be(PaymentStatus.Refunded);
        }

        [Fact]
        public void Resolve_When_Nothing_Refunded_Returns_Captured()
        {
            var summary = new NetsSummary { ReservedAmount = 10000, ChargedAmount = 10000 };

            NetsPaymentStatusResolver.Resolve(summary).Should().Be(PaymentStatus.Captured);
        }

        [Theory]
        [InlineData(10000, 0, 10000, PaymentStatus.Cancelled)]
        [InlineData(10000, 0, 0, PaymentStatus.Authorized)]
        [InlineData(0, 0, 0, PaymentStatus.Initialized)]
        public void Resolve_Without_Refund_Maps_Reserved_Charged_And_Cancelled(int reserved, int charged, int cancelled, PaymentStatus expected)
        {
            var summary = new NetsSummary { ReservedAmount = reserved, ChargedAmount = charged, CancelledAmount = cancelled };

            NetsPaymentStatusResolver.Resolve(summary).Should().Be(expected);
        }
    }
}
