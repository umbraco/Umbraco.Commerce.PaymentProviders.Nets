using System.Collections.Generic;
using System.Linq;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders
{
    internal static class NetsOrderReconciliation
    {
        /// <summary>
        /// Calculates the difference, in minor units, between the order transaction amount and the
        /// sum of the line item gross amounts. Because each line amount and the order amount are
        /// rounded to minor units independently, their sums can drift by a minor unit or two (for
        /// example when a percentage discount is applied). Nets Easy rejects a payment whose order
        /// amount does not exactly equal the sum of the line gross amounts, so this difference is
        /// used to build a rounding adjustment line. Returns 0 when the totals already match.
        /// See issue #838.
        /// </summary>
        /// <param name="orderAmount">The order transaction amount, in minor units.</param>
        /// <param name="items">The line items already added to the order.</param>
        public static long CalculateRoundingAdjustment(long orderAmount, IEnumerable<NetsOrderItem> items)
            => orderAmount - items.Sum(x => (long)x.GrossTotalAmount);
    }
}
