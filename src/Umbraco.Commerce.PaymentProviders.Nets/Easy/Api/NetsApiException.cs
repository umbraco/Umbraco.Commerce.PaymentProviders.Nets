using System;
using System.Linq;
using System.Net;
using Umbraco.Commerce.PaymentProviders.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Api
{
    public class NetsApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public NetsErrorResponse ValidationErrors { get; }
        public NetsServerErrorResponse ServerError { get; }

        public NetsApiException() { }

        public NetsApiException(string message) : base(message) { }

        public NetsApiException(string message, Exception innerException) : base(message, innerException) { }

        public NetsApiException(HttpStatusCode statusCode, NetsErrorResponse validationErrors)
         : base(FormatValidationErrorMessage(validationErrors))
        {
            StatusCode = statusCode;
            ValidationErrors = validationErrors;
        }

        public NetsApiException(HttpStatusCode statusCode, NetsServerErrorResponse serverError)
        : base(FormatServerErrorMessage(serverError))
        {
            StatusCode = statusCode;
            ServerError = serverError;
        }

        private static string FormatValidationErrorMessage(NetsErrorResponse errorResponse)
        {
            if (errorResponse?.Errors == null || errorResponse.Errors.Count == 0)
            {
                return "Validation failed, but no error details were provided.";
            }

            var errors = string.Join(" ", errorResponse.Errors
                   .Where(e => e.Value is { Count: >0 })
                   .Select(e => $"{e.Key}: {string.Join(", ", e.Value)}"));

            return $"Validation Errors: {errors}";
        }

        private static string FormatServerErrorMessage(NetsServerErrorResponse serverError)
        {
            if (serverError == null)
            {
                return "An unknown internal server error occurred.";
            }

            return $"Internal Server Error: {serverError.Message} (Code: {serverError.Code}, Source: {serverError.Source})";
        }
    }
}
