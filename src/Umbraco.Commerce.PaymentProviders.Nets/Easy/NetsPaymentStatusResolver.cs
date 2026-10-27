using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders
{
    internal static class NetsPaymentStatusResolver
    {
        /// <summary>
        /// Maps the amounts in a Nets payment summary to a Commerce payment status. A refund smaller
        /// than the charged amount is a partial refund, so it must not be reported as fully refunded.
        /// </summary>
        public static PaymentStatus Resolve(NetsSummary summary)
        {
            if (summary.RefundedAmount > 0)
                return summary.RefundedAmount < summary.ChargedAmount
                    ? PaymentStatus.PartiallyRefunded
                    : PaymentStatus.Refunded;

            if (summary.CancelledAmount > 0)
                return PaymentStatus.Cancelled;

            if (summary.ChargedAmount > 0)
                return PaymentStatus.Captured;

            if (summary.ReservedAmount > 0)
                return PaymentStatus.Authorized;

            return PaymentStatus.Initialized;
        }
    }
}
