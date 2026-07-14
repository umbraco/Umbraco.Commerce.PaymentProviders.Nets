using FluentAssertions;
using Umbraco.Commerce.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders.Nets.UnitTests
{
    public class NetsWebhookAuthorizationTests
    {
        // Issue #786: repeated form generation must not rotate the key, or callbacks
        // for earlier payments on the same order fail authorization.
        [Theory]
        [InlineData("6f1e6b0c-1c2d-4e3f-8a9b-0c1d2e3f4a5b")]
        [InlineData("any-existing-key")]
        public void ResolveAuthKey_When_Order_Has_A_Key_Reuses_It(string existing)
        {
            string resolved = NetsWebhookAuthorization.ResolveAuthKey(existing);

            resolved.Should().Be(existing);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ResolveAuthKey_When_Order_Has_No_Key_Generates_A_New_Guid(string existing)
        {
            string resolved = NetsWebhookAuthorization.ResolveAuthKey(existing);

            resolved.Should().NotBeNullOrEmpty();
            Guid.TryParse(resolved, out _).Should().BeTrue();
        }
    }
}
