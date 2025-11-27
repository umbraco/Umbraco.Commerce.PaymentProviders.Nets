using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsWebhookEvent
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("merchantId")]
        public int MerchantId { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("data")]
        public JsonObject Data { get; set; }
    }
}
