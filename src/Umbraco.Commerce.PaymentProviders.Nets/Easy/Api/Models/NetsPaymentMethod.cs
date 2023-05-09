﻿using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsPaymentMethod
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fee")]
        public NetsPaymentFee Fee { get; set; }
    }

    public class NetsPaymentFee
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }

        [JsonProperty("unitPrice")]
        public int UnitPrice { get; set; }

        [JsonProperty("taxRate")]
        public int TaxRate { get; set; }

        [JsonProperty("taxAmount")]
        public int TaxAmount { get; set; }

        [JsonProperty("grossTotalAmount")]
        public int GrossTotalAmount { get; set; }

        [JsonProperty("netTotalAmount")]
        public int NetTotalAmount { get; set; }
    }
}
