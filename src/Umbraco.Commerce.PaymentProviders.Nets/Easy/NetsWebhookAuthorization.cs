using System;

namespace Umbraco.Commerce.PaymentProviders
{
    internal static class NetsWebhookAuthorization
    {
        /// <summary>
        /// Resolves the webhook authorization key to register with Nets for an order. The key is
        /// stable per order: an existing key is reused so that webhooks registered for earlier
        /// payments on the same order remain verifiable, and a new key is only generated when the
        /// order does not yet have one. See issue #786.
        /// </summary>
        /// <param name="existingAuthKey">The authorization key already persisted on the order, if any.</param>
        public static string ResolveAuthKey(string existingAuthKey)
            => string.IsNullOrEmpty(existingAuthKey) ? Guid.NewGuid().ToString() : existingAuthKey;
    }
}
