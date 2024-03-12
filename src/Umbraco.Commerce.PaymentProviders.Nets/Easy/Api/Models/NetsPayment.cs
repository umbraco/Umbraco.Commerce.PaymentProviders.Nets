using System.Text.Json.Serialization;
using System;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsPaymentDetails
    {
        [JsonPropertyName("payment")]
        public NetsPayment Payment { get; set; }
    }

    public class NetsPayment : NetsPaymentBase
    {
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("paymentId")]
        public string PaymentId { get; set; }

        [JsonPropertyName("orderDetails")]
        public NetsOrderDetails OrderDetails { get; set; }

        [JsonPropertyName("summary")]
        public NetsSummary Summary { get; set; }
    }

    public class NetsOrderDetails
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }
    }

    public class NetsSummary
    {
        [JsonPropertyName("cancelledAmount")]
        public int CancelledAmount { get; set; }

        [JsonPropertyName("chargedAmount")]
        public int ChargedAmount { get; set; }

        [JsonPropertyName("refundedAmount")]
        public int RefundedAmount { get; set; }

        [JsonPropertyName("reservedAmount")]
        public int ReservedAmount { get; set; }
    }
}
