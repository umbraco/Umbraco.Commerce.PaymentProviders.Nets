using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsPaymentResult
    {
        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }
    }
}
