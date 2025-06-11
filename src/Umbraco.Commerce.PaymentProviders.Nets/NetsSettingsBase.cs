using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders
{
    public class NetsSettingsBase
    {
        [PaymentProviderSetting(SortOrder = 100)]
        public string ContinueUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 200)]
        public string CancelUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 300)]
        public string ErrorUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 400)]
        public string MerchantId { get; set; }

        [PaymentProviderSetting(SortOrder = 500)]
        public string Language { get; set; }

        [PaymentProviderSetting(SortOrder = 10000)]
        public bool TestMode { get; set; }
    }
}
