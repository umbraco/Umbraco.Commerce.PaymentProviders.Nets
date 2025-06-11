using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders
{
    public class NetsEasySettingsBase : NetsSettingsBase
    {
        [PaymentProviderSetting(SortOrder = 1000)]
        public string PaymentMethods { get; set; }

        [PaymentProviderSetting(SortOrder = 1100)]
        public string BillingCompanyPropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 1200)]
        public string BillingPhonePropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 1300)]
        public string ShippingAddressLine1PropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 1400)]
        public string ShippingAddressLine2PropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 1500)]
        public string ShippingAddressZipCodePropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 1600)]
        public string ShippingAddressCityPropertyAlias { get; set; }

        [PaymentProviderSetting(SortOrder = 1700)]
        public string TermsUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 1800)]
        public string MerchantTermsUrl { get; set; }

        [PaymentProviderSetting(SortOrder = 3100)]
        public string LiveSecretKey { get; set; }

        [PaymentProviderSetting(SortOrder = 3200)]
        public string LiveCheckoutKey { get; set; }

        [PaymentProviderSetting(SortOrder = 3300)]
        public string TestSecretKey { get; set; }

        [PaymentProviderSetting(SortOrder = 3400)]
        public string TestCheckoutKey { get; set; }
    }
}
