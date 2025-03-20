using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsErrorResponse
    {
        [JsonPropertyName("errors")]
        public Dictionary<string, List<string>> Errors { get; set; }
    }
}
