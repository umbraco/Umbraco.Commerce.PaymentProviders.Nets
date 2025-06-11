using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsPaymentResult
    {
        [JsonPropertyName("paymentId")]
        public string PaymentId { get; set; }
    }
}
