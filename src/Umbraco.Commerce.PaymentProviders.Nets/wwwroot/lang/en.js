export default {
    ucPaymentProviders: {
        // settings
        'netsEasyCheckoutOnetimeLabel': 'Nets Easy (One Time)',
        'netsEasyCheckoutOnetimeDescription': 'Nets Easy payment provider for one time payments',

        'netsEasyCheckoutOnetimeSettingsContinueUrlLabel': 'Continue URL',
        'netsEasyCheckoutOnetimeSettingsContinueUrlDescription': '[Required] The URL to continue to after this provider has done processing. eg: /continue/',
        'netsEasyCheckoutOnetimeSettingsCancelUrlLabel': 'Cancel URL',
        'netsEasyCheckoutOnetimeSettingsCancelUrlDescription': 'The URL to return to if the payment attempt is canceled. eg: /cart/',
        'netsEasyCheckoutOnetimeSettingsErrorUrlLabel': 'Error URL',
        'netsEasyCheckoutOnetimeSettingsErrorUrlDescription': 'The URL to return to if the payment attempt errors. eg: /error/',

        'netsEasyCheckoutOnetimeSettingsMerchantIdLabel': 'Merchant ID',
        'netsEasyCheckoutOnetimeSettingsMerchantIdDescription': '[Required] Merchant ID supplied by Nets during registration',

        'netsEasyCheckoutOnetimeSettingsTestModeLabel': 'Test Mode',
        'netsEasyCheckoutOnetimeSettingsTestModeDescription': 'Set whether to process payments in test mode',

        'netsEasyCheckoutOnetimeSettingsLanguageLabel': 'Language',
        'netsEasyCheckoutOnetimeSettingsLanguageDescription': 'Language used in the payment window',

        'netsEasyCheckoutOnetimeSettingsAutoCaptureLabel': 'Auto Capture',
        'netsEasyCheckoutOnetimeSettingsAutoCaptureDescription': 'Flag indicating whether to immediately capture the payment, or whether to just authorize the payment for later (manual) capture',

        'netsEasyCheckoutOnetimeSettingsPaymentMethodsLabel': 'Accepted Payment Methods',
        'netsEasyCheckoutOnetimeSettingsPaymentMethodsDescription': 'A comma separated list of Payment Methods to accept',

        'netsEasyCheckoutOnetimeSettingsBillingCompanyPropertyAliasLabel': 'Billing Company Property Alias',
        'netsEasyCheckoutOnetimeSettingsBillingCompanyPropertyAliasDescription': 'The order property alias containing company of the billing address (optional)',

        'netsEasyCheckoutOnetimeSettingsBillingPhonePropertyAliasLabel': 'Billing Phone Property Alias',
        'netsEasyCheckoutOnetimeSettingsBillingPhonePropertyAliasDescription': 'The order property alias containing phone of the billing address',

        'netsEasyCheckoutOnetimeSettingsShippingAddressLine1PropertyAliasLabel': 'Shipping Address (Line 1) Property Alias',
        'netsEasyCheckoutOnetimeSettingsShippingAddressLine1PropertyAliasDescription': 'The order property alias containing line 1 of the shipping address',

        'netsEasyCheckoutOnetimeSettingsShippingAddressLine2PropertyAliasLabel': 'Shipping Address (Line 2) Property Alias',
        'netsEasyCheckoutOnetimeSettingsShippingAddressLine2PropertyAliasDescription': 'The order property alias containing line 2 of the shipping address',

        'netsEasyCheckoutOnetimeSettingsShippingAddressZipCodePropertyAliasLabel': 'Shipping Address Zip Code Property Alias',
        'netsEasyCheckoutOnetimeSettingsShippingAddressZipCodePropertyAliasDescription': 'The order property alias containing the zip code of the shipping address',

        'netsEasyCheckoutOnetimeSettingsShippingAddressCityPropertyAliasLabel': 'Shipping Address City Property Alias',
        'netsEasyCheckoutOnetimeSettingsShippingAddressCityPropertyAliasDescription': 'The order property alias containing the city of the shipping address',

        'netsEasyCheckoutOnetimeSettingsTermsUrlLabel': 'Terms URL',
        'netsEasyCheckoutOnetimeSettingsTermsUrlDescription': '[Required] The URL to the terms and conditions of your webshop',

        'netsEasyCheckoutOnetimeSettingsMerchantTermsUrlLabel': 'Merchant Terms URL',
        'netsEasyCheckoutOnetimeSettingsMerchantTermsUrlDescription': 'The URL to the privacy and cookie settings of your webshop',

        'netsEasyCheckoutOnetimeSettingsLiveSecretKeyLabel': 'Live Secret Key',
        'netsEasyCheckoutOnetimeSettingsLiveSecretKeyDescription': 'Your live Nets secret key',

        'netsEasyCheckoutOnetimeSettingsLiveCheckoutKeyLabel': 'Live Checkout Key',
        'netsEasyCheckoutOnetimeSettingsLiveCheckoutKeyDescription': 'Your live Nets checkout key',

        'netsEasyCheckoutOnetimeSettingsTestSecretKeyLabel': 'Test Secret Key',
        'netsEasyCheckoutOnetimeSettingsTestSecretKeyDescription': 'Your test Nets secret key',

        'netsEasyCheckoutOnetimeSettingsTestCheckoutKeyLabel': 'Test Checkout Key',
        'netsEasyCheckoutOnetimeSettingsTestCheckoutKeyDescription': 'Your test Nets checkout key',

        // metadata
        'netsEasyCheckoutOnetimeMetaDataNetsEasyPaymentIdLabel': 'Nets (Easy) Payment ID',
        'netsEasyCheckoutOnetimeMetaDataNetsEasyChargeIdLabel': 'Nets (Easy) Charge ID',
        'netsEasyCheckoutOnetimeMetaDataNetsEasyRefundIdLabel': 'Nets (Easy) Refund ID',
        'netsEasyCheckoutOnetimeMetaDataNetsEasyCancelIdLabel': 'Nets (Easy) Cancel ID',
        'netsEasyCheckoutOnetimeMetaDataNetsEasyWebhookAuthKeyLabel': 'Nets (Easy) Webhook Authorization',
    },
};