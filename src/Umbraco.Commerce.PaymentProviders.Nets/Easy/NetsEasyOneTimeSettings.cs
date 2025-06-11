using Umbraco.Commerce.Core.PaymentProviders;

namespace Umbraco.Commerce.PaymentProviders
{
    public class NetsEasyOneTimeSettings : NetsEasySettingsBase
    {
        [PaymentProviderSetting(SortOrder = 5000)]
        public bool AutoCapture { get; set; }
    }
}
