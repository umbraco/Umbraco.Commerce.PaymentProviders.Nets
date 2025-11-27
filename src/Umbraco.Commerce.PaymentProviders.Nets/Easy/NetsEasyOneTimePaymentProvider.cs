using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Commerce.Common.Logging;
using Umbraco.Commerce.Core.Api;
using Umbraco.Commerce.Core.Models;
using Umbraco.Commerce.Core.PaymentProviders;
using Umbraco.Commerce.Core.Services;
using Umbraco.Commerce.Extensions;
using Umbraco.Commerce.PaymentProviders.Api;
using Umbraco.Commerce.PaymentProviders.Api.Models;
using Umbraco.Commerce.PaymentProviders.Nets;

namespace Umbraco.Commerce.PaymentProviders
{
    [PaymentProvider("nets-easy-checkout-onetime")]
    public class NetsEasyOneTimePaymentProvider : NetsPaymentProviderBase<NetsEasyOneTimePaymentProvider, NetsEasyOneTimeSettings>
    {
        private readonly IStoreService _storeService;

        public NetsEasyOneTimePaymentProvider(
            UmbracoCommerceContext ctx,
            ILogger<NetsEasyOneTimePaymentProvider> logger,
            IStoreService storeService)
            : base(ctx, logger)
        {
            _storeService = storeService;
        }

        public override bool CanCancelPayments => true;
        public override bool CanCapturePayments => true;
        public override bool CanRefundPayments => true;
        public override bool CanPartiallyRefundPayments => true;
        public override bool CanFetchPaymentStatus => true;

        // We'll finalize via webhook callback
        public override bool FinalizeAtContinueUrl => false;

        public override IEnumerable<TransactionMetaDataDefinition> TransactionMetaDataDefinitions => new[]
        {
            new TransactionMetaDataDefinition("netsEasyPaymentId"),
            new TransactionMetaDataDefinition("netsEasyChargeId"),
            new TransactionMetaDataDefinition("netsEasyRefundId"),
            new TransactionMetaDataDefinition("netsEasyCancelId"),
            new TransactionMetaDataDefinition("netsEasyWebhookAuthKey")
        };

        public override async Task<PaymentFormResult> GenerateFormAsync(PaymentProviderContext<NetsEasyOneTimeSettings> ctx, CancellationToken cancellationToken = default)
        {
            var currency = await Context.Services.CurrencyService.GetCurrencyAsync(ctx.Order.CurrencyId);
            var currencyCode = currency.Code.ToUpperInvariant();

            // Ensure currency has valid ISO 4217 code
            if (!Iso4217.CurrencyCodes.ContainsKey(currencyCode))
            {
                throw new NetsPaymentProviderGeneralException("Currency must be a valid ISO 4217 currency code: " + currency.Name);
            }

            var orderAmount = AmountToMinorUnits(ctx.Order.TransactionAmount.Value);

            var paymentMethods = ctx.Settings.PaymentMethods?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Where(x => !string.IsNullOrWhiteSpace(x))
                   .Select(s => s.Trim())
                   .ToArray();

            var paymentMethodId = ctx.Order.PaymentInfo.PaymentMethodId;
            var paymentMethod = paymentMethodId != null ? await Context.Services.PaymentMethodService.GetPaymentMethodAsync(paymentMethodId.Value) : null;

            string paymentId = string.Empty;
            string paymentFormLink = string.Empty;

            var webhookAuthKey = Guid.NewGuid().ToString();

            try
            {
                var clientConfig = GetNetsEasyClientConfig(ctx.Settings);
                var client = new NetsEasyClient(clientConfig);

                var items = ctx.Order.OrderLines.Select(x => new NetsOrderItem
                {
                    Reference = x.Sku,
                    Name = x.Name,
                    Quantity = (int)x.Quantity,
                    Unit = "pcs",
                    UnitPrice = (int)AmountToMinorUnits(x.UnitPrice.Value.WithoutTax),
                    TaxRate = (int)AmountToMinorUnits(x.TaxRate.Value * 100),
                    TaxAmount = (int)AmountToMinorUnits(x.TotalPrice.Value.Tax),
                    GrossTotalAmount = (int)AmountToMinorUnits(x.TotalPrice.Value.WithTax),
                    NetTotalAmount = (int)AmountToMinorUnits(x.TotalPrice.Value.WithoutTax)
                });

                var shippingMethod = await Context.Services.ShippingMethodService.GetShippingMethodAsync(ctx.Order.ShippingInfo.ShippingMethodId.Value);
                if (shippingMethod != null)
                {
                    items = items.Append(new NetsOrderItem
                    {
                        Reference = shippingMethod.Sku,
                        Name = shippingMethod.Name,
                        Quantity = 1,
                        Unit = "pcs",
                        UnitPrice = (int)AmountToMinorUnits(ctx.Order.ShippingInfo.TotalPrice.Value.WithoutTax),
                        TaxRate = (int)AmountToMinorUnits(ctx.Order.ShippingInfo.TaxRate.Value * 100),
                        TaxAmount = (int)AmountToMinorUnits(ctx.Order.ShippingInfo.TotalPrice.Value.Tax),
                        GrossTotalAmount = (int)AmountToMinorUnits(ctx.Order.ShippingInfo.TotalPrice.Value.WithTax),
                        NetTotalAmount = (int)AmountToMinorUnits(ctx.Order.ShippingInfo.TotalPrice.Value.WithoutTax)
                    });
                }

                // Check adjustments on subtotal price
                if (ctx.Order.SubtotalPrice.Adjustments.Count > 0)
                {
                    // Discounts
                    var discountAdjustments = ctx.Order.SubtotalPrice.Adjustments.OfType<DiscountAdjustment>();
                    if (discountAdjustments.Any())
                    {
                        foreach (var discount in discountAdjustments)
                        {
                            var taxRate = (discount.Price.Tax / discount.Price.WithoutTax) * 100;

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = discount.DiscountId.ToString(),
                                Name = discount.DiscountName,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(discount.Price.WithoutTax),
                                TaxRate = (int)AmountToMinorUnits(taxRate),
                                TaxAmount = (int)AmountToMinorUnits(discount.Price.Tax),
                                GrossTotalAmount = (int)AmountToMinorUnits(discount.Price.WithTax),
                                NetTotalAmount = (int)AmountToMinorUnits(discount.Price.WithoutTax)
                            });
                        }
                    }

                    // Custom price adjustments
                    var priceAdjustments = ctx.Order.SubtotalPrice.Adjustments.Except(discountAdjustments).OfType<PriceAdjustment>();
                    if (priceAdjustments.Any())
                    {
                        foreach (var adjustment in priceAdjustments)
                        {
                            var reference = Guid.NewGuid().ToString();
                            var taxRate = (adjustment.Price.Tax / adjustment.Price.WithoutTax) * 100;

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = reference,
                                Name = adjustment.Name,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(adjustment.Price.WithoutTax),
                                TaxRate = (int)AmountToMinorUnits(taxRate),
                                TaxAmount = (int)AmountToMinorUnits(adjustment.Price.Tax),
                                GrossTotalAmount = (int)AmountToMinorUnits(adjustment.Price.WithTax),
                                NetTotalAmount = (int)AmountToMinorUnits(adjustment.Price.WithoutTax)
                            });
                        }
                    }
                }

                // Check adjustments on total price
                if (ctx.Order.TotalPrice.Adjustments.Count > 0)
                {
                    // Discounts
                    var discountAdjustments = ctx.Order.TotalPrice.Adjustments.OfType<DiscountAdjustment>();
                    if (discountAdjustments.Any())
                    {
                        foreach (var discount in discountAdjustments)
                        {
                            var taxRate = (discount.Price.Tax / discount.Price.WithoutTax) * 100;

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = discount.DiscountId.ToString(),
                                Name = discount.DiscountName,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(discount.Price.WithoutTax),
                                TaxRate = (int)AmountToMinorUnits(taxRate),
                                TaxAmount = (int)AmountToMinorUnits(discount.Price.Tax),
                                GrossTotalAmount = (int)AmountToMinorUnits(discount.Price.WithTax),
                                NetTotalAmount = (int)AmountToMinorUnits(discount.Price.WithoutTax)
                            });
                        }
                    }

                    // Custom price adjustments
                    var priceAdjustments = ctx.Order.TotalPrice.Adjustments.Except(discountAdjustments).OfType<PriceAdjustment>();
                    if (priceAdjustments.Any())
                    {
                        foreach (var adjustment in priceAdjustments)
                        {
                            var reference = Guid.NewGuid().ToString();
                            var taxRate = (adjustment.Price.Tax / adjustment.Price.WithoutTax) * 100;

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = reference,
                                Name = adjustment.Name,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(adjustment.Price.WithoutTax),
                                TaxRate = (int)AmountToMinorUnits(taxRate),
                                TaxAmount = (int)AmountToMinorUnits(adjustment.Price.Tax),
                                GrossTotalAmount = (int)AmountToMinorUnits(adjustment.Price.WithTax),
                                NetTotalAmount = (int)AmountToMinorUnits(adjustment.Price.WithoutTax)
                            });
                        }
                    }
                }

                // Check adjustments on transaction amount
                if (ctx.Order.TransactionAmount.Adjustments.Count > 0)
                {
                    // Gift Card adjustments
                    var giftCardAdjustments = ctx.Order.TransactionAmount.Adjustments.OfType<GiftCardAdjustment>();
                    if (giftCardAdjustments.Any())
                    {
                        foreach (var giftcard in giftCardAdjustments)
                        {
                            items = items.Append(new NetsOrderItem
                            {
                                Reference = giftcard.GiftCardId.ToString(),
                                Name = giftcard.GiftCardCode, //$"Gift Card - {giftcard.Code}",
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(giftcard.Amount),
                                TaxRate = (int)AmountToMinorUnits(ctx.Order.TaxRate.Value * 100),
                                GrossTotalAmount = (int)AmountToMinorUnits(giftcard.Amount),
                                NetTotalAmount = (int)AmountToMinorUnits(giftcard.Amount)
                            });
                        }
                    }

                    // Custom Amount adjustments
                    var amountAdjustments = ctx.Order.TransactionAmount.Adjustments.Except(giftCardAdjustments).OfType<AmountAdjustment>();
                    if (amountAdjustments.Any())
                    {
                        foreach (var amount in amountAdjustments)
                        {
                            var reference = Guid.NewGuid().ToString();

                            items = items.Append(new NetsOrderItem
                            {
                                Reference = reference,
                                Name = amount.Name,
                                Quantity = 1,
                                Unit = "pcs",
                                UnitPrice = (int)AmountToMinorUnits(amount.Amount),
                                TaxRate = (int)AmountToMinorUnits(ctx.Order.TaxRate.Value * 100),
                                GrossTotalAmount = (int)AmountToMinorUnits(amount.Amount),
                                NetTotalAmount = (int)AmountToMinorUnits(amount.Amount)
                            });
                        }
                    }
                }

                string company = !string.IsNullOrWhiteSpace(ctx.Settings.BillingCompanyPropertyAlias)
                    ? ctx.Order.Properties[ctx.Settings.BillingCompanyPropertyAlias]
                    : string.Empty;

                var country = ctx.Order.ShippingInfo.CountryId.HasValue
                    ? await Context.Services.CountryService.GetCountryAsync(ctx.Order.ShippingInfo.CountryId.Value)
                    : null;

                var region = country != null ? new RegionInfo(country.Code) : null;
                var countryIsoCode = region?.ThreeLetterISORegionName;

                // If only partial data about the consumer is sent,
                // then the consumer will not be created in Easy.

                var consumer = new NetsConsumer
                {
                    Reference = ctx.Order.CustomerInfo.CustomerReference,
                    Email = ctx.Order.CustomerInfo.Email
                };

                // Address Line and City must not be empty and must be between 1 and 128 characters
                // Postal Code must not be empty and must be between 1 and 25 characters

                if (!string.IsNullOrWhiteSpace(ctx.Settings.ShippingAddressLine1PropertyAlias) &&
                    !string.IsNullOrWhiteSpace(ctx.Settings.ShippingAddressLine2PropertyAlias) &&
                    !string.IsNullOrWhiteSpace(ctx.Settings.ShippingAddressZipCodePropertyAlias) &&
                    !string.IsNullOrWhiteSpace(ctx.Settings.ShippingAddressCityPropertyAlias))
                {
                    consumer.ShippingAddress = new NetsAddress
                    {
                        Line1 = ctx.Order.Properties[ctx.Settings.ShippingAddressLine1PropertyAlias],
                        Line2 = ctx.Order.Properties[ctx.Settings.ShippingAddressLine2PropertyAlias],
                        PostalCode = ctx.Order.Properties[ctx.Settings.ShippingAddressZipCodePropertyAlias],
                        City = ctx.Order.Properties[ctx.Settings.ShippingAddressCityPropertyAlias],
                        Country = countryIsoCode
                    };
                }

                string phone = !string.IsNullOrWhiteSpace(ctx.Settings.BillingPhonePropertyAlias)
                    ? ctx.Order.Properties[ctx.Settings.BillingPhonePropertyAlias]
                    : string.Empty;

                phone = phone?
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Replace("-", "")
                    .Replace("(", "")
                    .Replace(")", "");

                if (!string.IsNullOrEmpty(phone) &&
                    Regex.Match(phone, @"^\+[0-9]{7,18}$").Success)
                {
                    var prefix = phone.Substring(0, 3);
                    var number = phone.Substring(3);

                    consumer.PhoneNumber = new NetsCustomerPhone
                    {
                        Prefix = prefix, // E.g "+45"
                        Number = number
                    };
                }

                // Fill either private person or company, not both.
                if (!string.IsNullOrWhiteSpace(company))
                {
                    consumer.Company = new NetsCompany
                    {
                        Name = company,
                        Contact = new NetsCustomerName
                        {
                            FirstName = ctx.Order.CustomerInfo.FirstName,
                            LastName = ctx.Order.CustomerInfo.LastName
                        }
                    };
                }
                else
                {
                    consumer.PrivatePerson = new NetsCustomerName
                    {
                        FirstName = ctx.Order.CustomerInfo.FirstName,
                        LastName = ctx.Order.CustomerInfo.LastName
                    };
                }

                var data = new NetsPaymentRequest
                {
                    Order = new NetsOrder
                    {
                        Reference = ctx.Order.OrderNumber,
                        Currency = currencyCode,
                        Amount = (int)orderAmount,
                        Items = items.ToArray()
                    },
                    Checkout = new NetsCheckout
                    {
                        Charge = ctx.Settings.AutoCapture,
                        IntegrationType = "HostedPaymentPage",
                        CancelUrl = ctx.Urls.CancelUrl,
                        ReturnUrl = ctx.Urls.ContinueUrl,
                        TermsUrl = ctx.Settings.TermsUrl,
                        MerchantTermsUrl = ctx.Settings.MerchantTermsUrl,
                        Appearance = new NetsAppearance
                        {
                            DisplayOptions = new NetsDisplayOptions
                            {
                                ShowMerchantName = true,
                                ShowOrderSummary = true
                            }
                        },
                        MerchantHandlesConsumerData = true,
                        Consumer = consumer
                    },
                    Notifications = new NetsNotifications
                    {
                        Webhooks = new NetsWebhook[]
                        {
                            new NetsWebhook
                            {
                                EventName = NetsEvents.PaymentCheckoutCompleted,
                                Url = ForceHttps(ctx.Urls.CallbackUrl),
                                Authorization = webhookAuthKey,
                                //Headers = new List<KeyValuePair<string, string>>()
                                //{
                                //    new KeyValuePair<string, string>("A", "a"),
                                //    new KeyValuePair<string, string>("B", "b")
                                //}
                            },
                            new NetsWebhook
                            {
                                EventName = NetsEvents.PaymentChargeCreated,
                                Url = ForceHttps(ctx.Urls.CallbackUrl),
                                Authorization = webhookAuthKey
                            },
                            new NetsWebhook
                            {
                                EventName = NetsEvents.PaymentRefundCompleted,
                                Url = ForceHttps(ctx.Urls.CallbackUrl),
                                Authorization = webhookAuthKey
                            },
                            new NetsWebhook
                            {
                                EventName = NetsEvents.PaymentCancelCreated,
                                Url = ForceHttps(ctx.Urls.CallbackUrl),
                                Authorization = webhookAuthKey
                            }
                        }
                    }
                };

                // Create payment
                var payment = await client.CreatePaymentAsync(data, cancellationToken).ConfigureAwait(false);

                // Get payment id
                paymentId = payment.PaymentId;

                var paymentDetails = await client.GetPaymentAsync(paymentId, cancellationToken).ConfigureAwait(false);
                if (paymentDetails != null)
                {
                    var uriBuilder = new UriBuilder(paymentDetails.Payment.Checkout.Url);
                    var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                    if (!string.IsNullOrEmpty(ctx.Settings.Language))
                    {
                        query["language"] = ctx.Settings.Language;
                    }

                    uriBuilder.Query = query.ToString();
                    paymentFormLink = uriBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Nets Easy - error creating payment.");
            }

            var checkoutKey = ctx.Settings.TestMode ? ctx.Settings.TestCheckoutKey : ctx.Settings.LiveCheckoutKey;

            return new PaymentFormResult()
            {
                MetaData = new Dictionary<string, string>
                {
                    { "netsEasyPaymentId", paymentId },
                    { "netsEasyWebhookAuthKey", webhookAuthKey }
                },
                Form = new PaymentForm(paymentFormLink, PaymentFormMethod.Get)
            };
        }

        public override async Task<CallbackResult> ProcessCallbackAsync(PaymentProviderContext<NetsEasyOneTimeSettings> ctx, CancellationToken cancellationToken = default)
        {
            try
            {
                // Process callback

                var webhookAuthKey = ctx.Order.Properties["netsEasyWebhookAuthKey"]?.Value;

                var clientConfig = GetNetsEasyClientConfig(ctx.Settings);
                var client = new NetsEasyClient(clientConfig);

                var netsEvent = await GetNetsWebhookEventAsync(ctx, webhookAuthKey, cancellationToken).ConfigureAwait(false);
                if (netsEvent != null)
                {
                    var paymentId = netsEvent.Data?["paymentId"]?.GetValue<string>();

                    NetsPaymentDetails payment = !string.IsNullOrEmpty(paymentId) ? await client.GetPaymentAsync(paymentId, cancellationToken).ConfigureAwait(false) : null;
                    if (payment != null)
                    {
                        var amount = (long)payment.Payment.OrderDetails.Amount;

                        if (netsEvent.Event == NetsEvents.PaymentCheckoutCompleted)
                        {
                            return CallbackResult.Ok(new TransactionInfo
                            {
                                TransactionId = paymentId,
                                AmountAuthorized = AmountFromMinorUnits(amount),
                                PaymentStatus = GetPaymentStatus(payment)
                            });
                        }
                        else if (netsEvent.Event == NetsEvents.PaymentChargeCreated)
                        {
                            var chargeId = netsEvent.Data?["chargeId"]?.GetValue<string>();

                            return CallbackResult.Ok(
                                new TransactionInfo
                                {
                                    TransactionId = paymentId,
                                    AmountAuthorized = AmountFromMinorUnits(amount),
                                    PaymentStatus = GetPaymentStatus(payment)
                                },
                                new Dictionary<string, string>
                                {
                                    { "netsEasyChargeId", chargeId }
                                });
                        }
                        else if (netsEvent.Event == NetsEvents.PaymentCancelCreated)
                        {
                            var cancelId = netsEvent.Data?["cancelId"]?.GetValue<string>();

                            return CallbackResult.Ok(
                                new TransactionInfo
                                {
                                    TransactionId = paymentId,
                                    AmountAuthorized = AmountFromMinorUnits(amount),
                                    PaymentStatus = GetPaymentStatus(payment)
                                },
                                new Dictionary<string, string>
                                {
                                    { "netsEasyCancelId", cancelId }
                                });
                        }
                        else if (netsEvent.Event == NetsEvents.PaymentRefundCompleted)
                        {
                            var refundId = netsEvent.Data?["refundId"]?.GetValue<string>();

                            return CallbackResult.Ok(
                                new TransactionInfo
                                {
                                    TransactionId = paymentId,
                                    AmountAuthorized = AmountFromMinorUnits(amount),
                                    PaymentStatus = await ctx.Order.CalculateRefundableAmountAsync() > 0 ?
                                        PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded,
                                },
                                new Dictionary<string, string>
                                {
                                    { "netsEasyRefundId", refundId }
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Nets Easy - ProcessCallback");
            }

            return CallbackResult.BadRequest();
        }

        public override async Task<ApiResult> FetchPaymentStatusAsync(PaymentProviderContext<NetsEasyOneTimeSettings> ctx, CancellationToken cancellationToken = default)
        {
            // Get payment: https://tech.netspayment.com/easy/api/paymentapi#getPayment

            try
            {
                var clientConfig = GetNetsEasyClientConfig(ctx.Settings);
                var client = new NetsEasyClient(clientConfig);

                var transactionId = ctx.Order.TransactionInfo.TransactionId;

                // Get payment
                var payment = await client.GetPaymentAsync(transactionId, cancellationToken).ConfigureAwait(false);
                if (payment != null)
                {
                    return new ApiResult()
                    {
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = GetTransactionId(payment),
                            PaymentStatus = GetPaymentStatus(payment)
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Nets Easy - FetchPaymentStatus");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult> CancelPaymentAsync(PaymentProviderContext<NetsEasyOneTimeSettings> ctx, CancellationToken cancellationToken = default)
        {
            // Cancel payment: https://tech.netspayment.com/easy/api/paymentapi#cancelPayment

            try
            {
                var clientConfig = GetNetsEasyClientConfig(ctx.Settings);
                var client = new NetsEasyClient(clientConfig);

                var transactionId = ctx.Order.TransactionInfo.TransactionId;

                var data = new
                {
                    amount = AmountToMinorUnits(ctx.Order.TransactionInfo.AmountAuthorized.Value)
                };

                // Cancel charge
                await client.CancelPaymentAsync(transactionId, data, cancellationToken).ConfigureAwait(false);

                return new ApiResult()
                {
                    TransactionInfo = new TransactionInfoUpdate()
                    {
                        TransactionId = transactionId,
                        PaymentStatus = PaymentStatus.Cancelled
                    }
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Nets Easy - CancelPayment");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult> CapturePaymentAsync(PaymentProviderContext<NetsEasyOneTimeSettings> ctx, CancellationToken cancellationToken = default)
        {
            // Charge payment: https://tech.netspayment.com/easy/api/paymentapi#chargePayment

            try
            {
                var clientConfig = GetNetsEasyClientConfig(ctx.Settings);
                var client = new NetsEasyClient(clientConfig);

                var transactionId = ctx.Order.TransactionInfo.TransactionId;

                var data = new
                {
                    amount = AmountToMinorUnits(ctx.Order.TransactionInfo.AmountAuthorized.Value)
                };

                var result = await client.ChargePaymentAsync(transactionId, data, cancellationToken).ConfigureAwait(false);
                if (result != null)
                {
                    return new ApiResult()
                    {
                        MetaData = new Dictionary<string, string>
                        {
                            { "netsEasyChargeId", result.ChargeId }
                        },
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = transactionId,
                            PaymentStatus = PaymentStatus.Captured
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Nets Easy - CapturePayment");
            }

            return ApiResult.Empty;
        }

        public override async Task<ApiResult> RefundPaymentAsync(PaymentProviderContext<NetsEasyOneTimeSettings> context, PaymentProviderOrderRefundRequest refundRequest, CancellationToken cancellationToken = default)
        {
            // Refund payment: https://tech.netspayment.com/easy/api/paymentapi#refundPayment
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(refundRequest);

            try
            {
                NetsEasyClientConfig clientConfig = GetNetsEasyClientConfig(context.Settings);
                NetsEasyClient client = new(clientConfig);

                string transactionId = context.Order.TransactionInfo.TransactionId;
                string chargeId = context.Order.Properties["netsEasyChargeId"]?.Value;

                var refundedOrderLineWithQuantities = (from orderLine in context.Order.OrderLines
                   join refundedOrderLine in refundRequest.OrderLines on orderLine.Id equals refundedOrderLine.OrderLineId
                   select new
                   {
                       OrderLine = orderLine,
                       RefundedQuantity = refundedOrderLine.Quantity,
                   }).ToList();

                IEnumerable<NetsOrderItem> refundItems = [
                    new NetsOrderItem
                    {
                        Reference = "refund",
                        Name = "refund",
                        Quantity = 1,
                        Unit = "pcs",
                        UnitPrice = (int)AmountToMinorUnits(refundRequest.RefundAmount),
                        TaxRate = 0,
                        TaxAmount = 0,
                        GrossTotalAmount = (int)AmountToMinorUnits(refundRequest.RefundAmount),
                        NetTotalAmount = (int)AmountToMinorUnits(refundRequest.RefundAmount),
                    },
                ];

                var data = new
                {
                    invoice = context.Order.OrderNumber,
                    amount = AmountToMinorUnits(refundRequest.RefundAmount),
                    orderItems = refundItems,
                };

                NetsRefund result = await client.RefundPaymentAsync(chargeId, data, cancellationToken).ConfigureAwait(false);
                if (result != null)
                {
                    return new ApiResult()
                    {
                        MetaData = new Dictionary<string, string>
                        {
                            { "netsEasyRefundId", result.RefundId }
                        },
                        TransactionInfo = new TransactionInfoUpdate()
                        {
                            TransactionId = transactionId,
                            PaymentStatus = await context.Order.CalculateRefundableAmountAsync() == refundRequest.RefundAmount ?
                                PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded,
                        },
                    };
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Nets Easy - RefundPayment");
            }

            return ApiResult.Empty;
        }

        protected string GetTransactionId(NetsPaymentDetails paymentDetails)
        {
            return paymentDetails?.Payment?.PaymentId;
        }

        protected PaymentStatus GetPaymentStatus(NetsPaymentDetails paymentDetails)
        {
            var payment = paymentDetails.Payment;

            if (payment.Summary.RefundedAmount > 0)
                return PaymentStatus.Refunded;

            if (payment.Summary.CancelledAmount > 0)
                return PaymentStatus.Cancelled;

            if (payment.Summary.ChargedAmount > 0)
                return PaymentStatus.Captured;

            if (payment.Summary.ReservedAmount > 0)
                return PaymentStatus.Authorized;

            return PaymentStatus.Initialized;
        }

        protected NetsEasyClientConfig GetNetsEasyClientConfig(NetsEasySettingsBase settings)
        {
            var prefix = settings.TestMode ? "test-secret-key-" : "live-secret-key-";
            var secretKey = settings.TestMode ? settings.TestSecretKey : settings.LiveSecretKey;
            var auth = secretKey?.Trim().Replace(prefix, string.Empty);

            return new NetsEasyClientConfig
            {
                BaseUrl = $"https://{(settings.TestMode ? "test." : "")}api.dibspayment.eu",
                Authorization = auth
            };
        }
    }
}
