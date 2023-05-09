using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsRefund
    {
        [JsonProperty("refundId")]
        public string RefundId { get; set; }
    }
}
