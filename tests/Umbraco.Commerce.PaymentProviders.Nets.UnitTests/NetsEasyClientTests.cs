using System.Text.Json;
using FluentAssertions;
using Flurl.Http.Testing;
using Umbraco.Commerce.PaymentProviders.Api;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Nets.UnitTests
{
    // Every NetsEasyClient call is run against Flurl's HttpTest, so no request leaves the process.
    public sealed class NetsEasyClientTests : IDisposable
    {
        private const string BaseUrl = "https://test.api.dibspayment.eu";
        private const string SecretKey = "abc123";

        private readonly HttpTest _httpTest = new();
        private readonly NetsEasyClient _client = new(new NetsEasyClientConfig { BaseUrl = BaseUrl, Authorization = SecretKey });

        public void Dispose()
        {
            _httpTest.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task CreatePayment_Posts_CamelCase_Body_Without_Nulls()
        {
            _httpTest.RespondWith("""{"paymentId":"p1"}""");

            NetsPaymentResult result = await _client.CreatePaymentAsync(new NetsPaymentRequest
            {
                Order = new NetsOrder
                {
                    Reference = "ORDER-1",
                    Currency = "DKK",
                    Amount = 12500,
                    Items =
                    [
                        new NetsOrderItem { Reference = "SKU1", Name = "Thing", Quantity = 1, Unit = "pcs", UnitPrice = 10000, TaxRate = 2500, TaxAmount = 2500, GrossTotalAmount = 12500, NetTotalAmount = 10000 },
                    ],
                },
                Checkout = new NetsCheckout
                {
                    Charge = false,
                    IntegrationType = "HostedPaymentPage",
                    ReturnUrl = "https://shop.example.com/continue",
                    CancelUrl = "https://shop.example.com/cancel",
                    TermsUrl = "https://shop.example.com/terms",
                },
                Notifications = new NetsNotifications
                {
                    Webhooks =
                    [
                        new NetsWebhook { EventName = "payment.checkout.completed", Url = "https://shop.example.com/callback", Authorization = "webhook-key" },
                    ],
                },
            });

            result.PaymentId.Should().Be("p1");
            _httpTest.ShouldHaveCalled($"{BaseUrl}/v1/payments/")
                .WithVerb(HttpMethod.Post)
                .WithHeader("Authorization", SecretKey)
                .WithContentType("application/json")
                .Times(1);

            using JsonDocument body = JsonDocument.Parse(_httpTest.CallLog[0].RequestBody!);
            JsonElement root = body.RootElement;
            root.GetProperty("order").GetProperty("reference").GetString().Should().Be("ORDER-1");
            root.GetProperty("order").GetProperty("currency").GetString().Should().Be("DKK");
            root.GetProperty("order").GetProperty("amount").GetInt32().Should().Be(12500);

            JsonElement item = root.GetProperty("order").GetProperty("items")[0];
            item.GetProperty("grossTotalAmount").GetInt32().Should().Be(12500);
            item.GetProperty("netTotalAmount").GetInt32().Should().Be(10000);
            item.GetProperty("taxRate").GetInt32().Should().Be(2500);

            JsonElement checkout = root.GetProperty("checkout");
            checkout.GetProperty("integrationType").GetString().Should().Be("HostedPaymentPage");
            checkout.GetProperty("returnUrl").GetString().Should().Be("https://shop.example.com/continue");
            checkout.GetProperty("charge").GetBoolean().Should().BeFalse();
            checkout.TryGetProperty("consumer", out _).Should().BeFalse("null values are not sent");
            checkout.TryGetProperty("merchantTermsUrl", out _).Should().BeFalse("null values are not sent");

            JsonElement webhook = root.GetProperty("notifications").GetProperty("webhooks")[0];
            webhook.GetProperty("eventName").GetString().Should().Be("payment.checkout.completed");
            webhook.GetProperty("authorization").GetString().Should().Be("webhook-key");

            root.TryGetProperty("paymentMethods", out _).Should().BeFalse("null values are not sent");
        }

        [Fact]
        public async Task GetPayment_Reads_Summary_And_Ignores_Unknown_Fields()
        {
            _httpTest.RespondWith("""
                {"payment":{"paymentId":"p1","somethingNew":{"x":1},
                 "orderDetails":{"amount":12500,"currency":"DKK","reference":"ORDER-1"},
                 "summary":{"reservedAmount":12500,"chargedAmount":12500,"refundedAmount":2500}}}
                """);

            NetsPaymentDetails details = await _client.GetPaymentAsync("p1");

            _httpTest.ShouldHaveCalled($"{BaseUrl}/v1/payments/p1").WithVerb(HttpMethod.Get).WithHeader("Authorization", SecretKey).Times(1);
            details.Payment.PaymentId.Should().Be("p1");
            details.Payment.OrderDetails.Amount.Should().Be(12500);
            details.Payment.Summary.ChargedAmount.Should().Be(12500);
            details.Payment.Summary.RefundedAmount.Should().Be(2500);
            NetsPaymentStatusResolver.Resolve(details.Payment.Summary).Should().Be(Umbraco.Commerce.Core.Models.PaymentStatus.PartiallyRefunded);
        }

        [Fact]
        public async Task ChargePayment_Posts_To_Charges()
        {
            _httpTest.RespondWith("""{"chargeId":"c1"}""");

            NetsCharge charge = await _client.ChargePaymentAsync("p1", new { amount = 12500 });

            charge.ChargeId.Should().Be("c1");
            _httpTest.ShouldHaveCalled($"{BaseUrl}/v1/payments/p1/charges")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody("{\"amount\":12500}")
                .Times(1);
        }

        [Fact]
        public async Task RefundPayment_Posts_To_Charge_Refunds()
        {
            _httpTest.RespondWith("""{"refundId":"r1"}""");

            NetsRefund refund = await _client.RefundPaymentAsync("c1", new { amount = 2500 });

            refund.RefundId.Should().Be("r1");
            _httpTest.ShouldHaveCalled($"{BaseUrl}/v1/charges/c1/refunds")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody("{\"amount\":2500}")
                .Times(1);
        }

        [Fact]
        public async Task CancelPayment_Posts_To_Cancels_And_Accepts_Empty_Response()
        {
            _httpTest.RespondWith(string.Empty, 204);

            string result = await _client.CancelPaymentAsync("p1", new { amount = 12500 });

            result.Should().BeEmpty();
            _httpTest.ShouldHaveCalled($"{BaseUrl}/v1/payments/p1/cancels")
                .WithVerb(HttpMethod.Post)
                .WithRequestBody("{\"amount\":12500}")
                .Times(1);
        }

        [Fact]
        public async Task Api_Error_Is_Rethrown()
        {
            _httpTest.RespondWith("""{"errors":{"amount":["bad"]}}""", 400);

            Func<Task> act = () => _client.ChargePaymentAsync("p1", new { amount = 1 });

            await act.Should().ThrowAsync<Flurl.Http.FlurlHttpException>();
        }
    }
}
