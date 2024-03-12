
using System.Text.Json;
using FluentAssertions;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Nets.UnitTests
{
    public class WebhookSerializationTests
    {
        [Fact]
        public void NetsWebhookEvent_Deserialization_Should_Succeed()
        {
            // preparation
            string json = "{\"id\":\"02650000659cfbb0b67254332c75f8d7\",\"merchantId\":\"100056734\",\"timestamp\":\"2024-01-09T07:54:24.9043+00:00\",\"event\":\"payment.charge.created\",\"data\":{\"chargeId\":\"02650000659cfbb0b67254332c75f8d7\",\"orderItems\":[{\"grossTotalAmount\":5520,\"name\":\"Nemi - Cardamom Chai - 15 tea bags\",\"netTotalAmount\":4600,\"quantity\":1.0,\"reference\":\"NM001-15\",\"taxRate\":2000,\"taxAmount\":920,\"unit\":\"pcs\",\"unitPrice\":4600},{\"grossTotalAmount\":0,\"name\":\"Pickup\",\"netTotalAmount\":0,\"quantity\":1.0,\"reference\":\"1623\",\"taxRate\":2000,\"taxAmount\":0,\"unit\":\"pcs\",\"unitPrice\":0}],\"reservationId\":\"1bc898d7b4644a4d9469e4c1c3140169\",\"reconciliationReference\":\"re7fmVG4gCRmbv2rEM4r0PsiO\",\"amount\":{\"amount\":5520,\"currency\":\"EUR\"},\"paymentId\":\"028e0000659cfb8eb17d1a589002cd3a\"}}";
            JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
            // execution
            NetsWebhookEvent? deserialized = JsonSerializer.Deserialize<NetsWebhookEvent>(json, options);

            // asserts
            deserialized.Should().NotBeNull();
            deserialized!.Data.Should().NotBeNull();
        }
    }
}
