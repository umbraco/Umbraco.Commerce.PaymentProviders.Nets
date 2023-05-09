using Newtonsoft.Json;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsCharge
    {
        [JsonProperty("chargeId")]
        public string ChargeId { get; set; }

        [JsonProperty("invoice")]
        public NetsInvoice Invoice { get; set; }
    }

    public class NetsInvoice
    {
        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }
    }
}
