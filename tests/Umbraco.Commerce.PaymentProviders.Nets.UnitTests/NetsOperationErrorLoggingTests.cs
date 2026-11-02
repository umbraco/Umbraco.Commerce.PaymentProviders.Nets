using FluentAssertions;
using Umbraco.Commerce.Common.Logging;

namespace Umbraco.Commerce.PaymentProviders.Nets.UnitTests
{
    public class NetsOperationErrorLoggingTests
    {
        [Fact]
        public void A_Failed_Operation_Is_Logged_With_The_Exception_And_The_Order_It_Was_For()
        {
            var logger = new RecordingLogger();
            var provider = new TestableProvider(logger);
            var exception = new InvalidOperationException("Nets returned 400");

            provider.LogOperationError(exception, "CancelPayment", "ORDER-0001", "CART-0001", "pay_123");

            RecordingLogger.Entry entry = logger.Entries.Should().ContainSingle().Subject;
            entry.Exception.Should().BeSameAs(exception);
            entry.Values.Should().Equal("CancelPayment", "ORDER-0001", "CART-0001", "pay_123");
        }

        private sealed class TestableProvider : NetsEasyOneTimePaymentProvider
        {
            public TestableProvider(ILogger<NetsEasyOneTimePaymentProvider> logger)
                : base(null!, logger, null!)
            {
            }
        }

        /// <summary>
        /// Records the errors logged; nothing else in these tests is logged.
        /// </summary>
        private sealed class RecordingLogger : ILogger<NetsEasyOneTimePaymentProvider>
        {
            public List<Entry> Entries { get; } = new();

            public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
                => Entries.Add(new Entry(exception, messageTemplate, propertyValues));

            public void Error(Exception exception, string message) => Error(exception, message, []);

            public void Error(Exception exception) => Error(exception, string.Empty, []);

            public void Error(string message) => Error(null!, message, []);

            public void Error(string messageTemplate, params object[] propertyValues) => Error(null!, messageTemplate, propertyValues);

            public bool IsEnabled(LogLevel level) => true;

            public void Fatal(Exception exception, string message) { }

            public void Fatal(Exception exception) { }

            public void Fatal(string message) { }

            public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues) { }

            public void Fatal(string messageTemplate, params object[] propertyValues) { }

            public void Warn(string message) { }

            public void Warn(string messageTemplate, params object[] propertyValues) { }

            public void Warn(Exception exception, string message) { }

            public void Warn(Exception exception, string messageTemplate, params object[] propertyValues) { }

            public void Info(string message) { }

            public void Info(string messageTemplate, params object[] propertyValues) { }

            public void Debug(string message) { }

            public void Debug(string messageTemplate, params object[] propertyValues) { }

            public void Verbose(string message) { }

            public void Verbose(string messageTemplate, params object[] propertyValues) { }

            internal sealed record Entry(Exception Exception, string MessageTemplate, object[] Values);
        }
    }
}
