using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsPaymentBase
    {
        [JsonProperty("checkout")]
        public NetsPaymentCheckout Checkout { get; set; }
    }

    public class NetsPaymentCheckout
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
