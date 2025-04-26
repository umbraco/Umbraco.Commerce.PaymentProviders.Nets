using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Umbraco.Commerce.PaymentProviders.Api.Models;
using Umbraco.Commerce.PaymentProviders.Nets.Easy.Api;

namespace Umbraco.Commerce.PaymentProviders.Api
{
    public class NetsEasyClient
    {
        private readonly NetsEasyClientConfig _config;

        public NetsEasyClient(NetsEasyClientConfig config)
        {
            _config = config;
        }

        public async Task<NetsPaymentResult> CreatePaymentAsync(NetsPaymentRequest data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync("/v1/payments/", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<NetsPaymentResult>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<NetsPaymentDetails> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/v1/payments/{paymentId}", async (req, ct) => await req
                .GetJsonAsync<NetsPaymentDetails>(cancellationToken: ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> CancelPaymentAsync(string paymentId, object data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/v1/payments/{paymentId}/cancels", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<string>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<NetsCharge> ChargePaymentAsync(string paymentId, object data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/v1/payments/{paymentId}/charges", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<NetsCharge>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<NetsRefund> RefundPaymentAsync(string chargeId, object data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/v1/charges/{chargeId}/refunds", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<NetsRefund>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<TResult> RequestAsync<TResult>(string url, Func<IFlurlRequest, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            try
            {
                var req = new FlurlRequest(_config.BaseUrl + url)
                        .WithSettings(x =>
                        {
                            var jsonSettings = new System.Text.Json.JsonSerializerOptions
                            {
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                            };
                            x.JsonSerializer = new CustomFlurlJsonSerializer(jsonSettings);
                        })
                        .WithHeader("Authorization", _config.Authorization);

                return await func.Invoke(req, cancellationToken).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                HttpStatusCode statusCode = ex.StatusCode.HasValue ? (HttpStatusCode)ex.StatusCode.Value : HttpStatusCode.BadRequest;

                if (statusCode == HttpStatusCode.Unauthorized)
                {
                    throw new NetsApiException(HttpStatusCode.Unauthorized, new NetsErrorResponse
                    {
                        Errors = new Dictionary<string, List<string>> { { "Authorization", new List<string> { "Unauthorized request - Missing or invalid secret." } } }
                    });
                }

                if (statusCode == HttpStatusCode.InternalServerError)
                {
                    NetsServerErrorResponse netsServerErrorResponse = null;
                    try
                    {
                        netsServerErrorResponse = await ex.GetResponseJsonAsync<NetsServerErrorResponse>().ConfigureAwait(false);
                    }
                    catch
                    {
                        netsServerErrorResponse = new NetsServerErrorResponse
                        {
                            Message = "An unexpected internal server error occurred.",
                            Code = "Unknown",
                            Source = "Unknown"
                        };
                    }

                    throw new NetsApiException(statusCode, netsServerErrorResponse);
                }

                if (statusCode == HttpStatusCode.BadRequest)
                {
                    NetsErrorResponse netsErrorResponse = null;
                    try
                    {
                        netsErrorResponse = await ex.GetResponseJsonAsync<NetsErrorResponse>().ConfigureAwait(false);
                    }
                    catch
                    {
                        netsErrorResponse = new NetsErrorResponse
                        {
                            Errors = new Dictionary<string, List<string>> { { "UnknownError", new List<string> { "A validation error occurred, but details could not be retrieved." } } }
                        };
                    }

                    throw new NetsApiException(statusCode, netsErrorResponse);
                }

                string rawResponse = await ex.GetResponseStringAsync().ConfigureAwait(false);
                throw new NetsApiException($"Unexpected error from Nets API (Status {statusCode}): {rawResponse}");
            }
        }
    }
}
