using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsRefund
    {
        [JsonPropertyName("refundId")]
        public string RefundId { get; set; }
    }
}
