using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsPaymentBase
    {
        [JsonPropertyName("checkout")]
        public NetsPaymentCheckout Checkout { get; set; }
    }

    public class NetsPaymentCheckout
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
