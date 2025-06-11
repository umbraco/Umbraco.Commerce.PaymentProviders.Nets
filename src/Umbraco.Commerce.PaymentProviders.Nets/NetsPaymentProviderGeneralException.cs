using System;

namespace Umbraco.Commerce.PaymentProviders.Nets
{
    public class NetsPaymentProviderGeneralException : Exception
    {
        public NetsPaymentProviderGeneralException(string message) : base(message)
        {
        }

        public NetsPaymentProviderGeneralException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public NetsPaymentProviderGeneralException()
        {
        }
    }
}
