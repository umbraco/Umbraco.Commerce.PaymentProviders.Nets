using FluentAssertions;
using Flurl.Http.Testing;
using Umbraco.Commerce.PaymentProviders.Api;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Nets.UnitTests
{
    // NetsEasyClient calls are run against Flurl's HttpTest, so no request leaves the process.
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
        public async Task CancelPayment_Accepts_204_No_Content()
        {
            _httpTest.RespondWith(string.Empty, 204);

            string result = await _client.CancelPaymentAsync("p1", new { amount = 12500 }).ConfigureAwait(true);

            result.Should().BeEmpty();
            _httpTest.ShouldHaveCalled($"{BaseUrl}/v1/payments/p1/cancels")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }
    }
}
