using System.Text.Json.Serialization;

namespace Umbraco.Commerce.PaymentProviders.Api.Models
{
    public class NetsCharge
    {
        [JsonPropertyName("chargeId")]
        public string ChargeId { get; set; }

        [JsonPropertyName("invoice")]
        public NetsInvoice Invoice { get; set; }
    }

    public class NetsInvoice
    {
        [JsonPropertyName("invoiceNumber")]
        public string InvoiceNumber { get; set; }
    }
}
