using FluentAssertions;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Nets.UnitTests
{
    public class NetsEasyClientConfigTests
    {
        [Theory]
        [InlineData(true, "https://test.api.dibspayment.eu", "abc")]
        [InlineData(false, "https://api.dibspayment.eu", "xyz")]
        public void Config_Uses_Mode_Url_And_Strips_Key_Prefix(bool testMode, string expectedUrl, string expectedAuth)
        {
            NetsEasyOneTimeSettings settings = new()
            {
                TestMode = testMode,
                TestSecretKey = " test-secret-key-abc ",
                LiveSecretKey = "live-secret-key-xyz",
            };

            NetsEasyClientConfig config = new TestableProvider().ClientConfig(settings);

            config.BaseUrl.Should().Be(expectedUrl);
            config.Authorization.Should().Be(expectedAuth);
        }

        [Fact]
        public void Config_Keeps_Key_Without_Prefix()
        {
            NetsEasyClientConfig config = new TestableProvider().ClientConfig(new NetsEasyOneTimeSettings { TestMode = true, TestSecretKey = "abc" });

            config.Authorization.Should().Be("abc");
        }

        [Theory]
        [InlineData("http://shop.example.com/callback", "https://shop.example.com/callback")]
        [InlineData("http://shop.example.com:8080/callback?x=1", "https://shop.example.com:8080/callback?x=1")]
        [InlineData("https://shop.example.com/callback", "https://shop.example.com/callback")]
        public void ForceHttps_Switches_Scheme_And_Keeps_Custom_Port(string url, string expected)
        {
            TestableProvider.Https(url).Should().Be(expected);
        }

        private sealed class TestableProvider : NetsEasyOneTimePaymentProvider
        {
            public TestableProvider()
                : base(null!, null!, null!)
            {
            }

            public NetsEasyClientConfig ClientConfig(NetsEasyOneTimeSettings settings) => GetNetsEasyClientConfig(settings);

            public static string Https(string url) => ForceHttps(url);
        }
    }
}
