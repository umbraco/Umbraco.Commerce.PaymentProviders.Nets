using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders
{
    public abstract class NetsPaymentProviderBase<TSelf, TSettings> : PaymentProviderBase<TSettings>
        where TSelf : NetsPaymentProviderBase<TSelf, TSettings>
        where TSettings : NetsSettingsBase, new()
    {
        protected ILogger<TSelf> Logger { get; }

        protected NetsPaymentProviderBase(
            UmbracoCommerceContext ctx,
            ILogger<TSelf> logger)
            : base(ctx)
        {
            Logger = logger;
        }

        public override string GetCancelUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.CancelUrl.MustNotBeNull("settings.CancelUrl");

            return ctx.Settings.CancelUrl;
        }

        public override string GetContinueUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.ContinueUrl.MustNotBeNull("settings.ContinueUrl");

            return ctx.Settings.ContinueUrl;
        }

        public override string GetErrorUrl(PaymentProviderContext<TSettings> ctx)
        {
            ctx.Settings.MustNotBeNull("settings");
            ctx.Settings.ErrorUrl.MustNotBeNull("settings.ErrorUrl");

            return ctx.Settings.ErrorUrl;
        }

        protected async Task<NetsWebhookEvent> GetNetsWebhookEventAsync(PaymentProviderContext<TSettings> ctx, string webhookAuthorization, CancellationToken cancellationToken = default)
        {
            NetsWebhookEvent netsWebhookEvent = null;

            if (ctx.AdditionalData.ContainsKey("Vendr_NetsEasyWebhookEvent"))
            {
                netsWebhookEvent = (NetsWebhookEvent)ctx.AdditionalData["Vendr_NetsEasyWebhookEvent"];
            }
            else
            {
                try
                {
                    if (ctx.HttpContext.Request.Body.CanSeek)
                    {
                        ctx.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                    }

                    using (var reader = new StreamReader(ctx.HttpContext.Request.Body))
                    {
                        var json = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(json))
                        {
                            // Verify "Authorization" header returned from webhook
                            VerifyAuthorization(ctx.HttpContext.Request, webhookAuthorization);

                            netsWebhookEvent = JsonSerializer.Deserialize<NetsWebhookEvent>(json);

                            ctx.AdditionalData.Add("Vendr_NetsEasyWebhookEvent", netsWebhookEvent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Nets Easy - GetNetsWebhookEvent");
                }
            }

            return netsWebhookEvent;
        }

        protected static string ForceHttps(string url)
        {
            var uri = new UriBuilder(url);

            var hadDefaultPort = uri.Uri.IsDefaultPort;
            uri.Scheme = Uri.UriSchemeHttps;
            uri.Port = hadDefaultPort ? -1 : uri.Port;

            return uri.ToString();
        }

        private void VerifyAuthorization(HttpRequest request, string webhookAuthorization)
        {
            string authHeader = request.Headers.Authorization;

            if (authHeader == null)
                throw new Exception("The authorization header is not present in the webhook event.");

            if (authHeader != webhookAuthorization)
                throw new Exception("The authorization in the webhook event could not be verified.");
        }
    }
}
