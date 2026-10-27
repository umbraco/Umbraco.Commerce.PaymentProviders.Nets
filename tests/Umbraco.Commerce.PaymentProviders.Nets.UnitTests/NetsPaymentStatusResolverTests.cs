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
    }
}
