using Domain.InterFaces;
using Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.Services
{
    /// <summary>
    /// Integrates with the HyperPay gateway for Mada card payments (Saudi Arabia).
    ///
    /// Test mode (IsTestMode=true in appsettings) returns mock responses so development
    /// can proceed without real credentials. Set IsTestMode=false and supply live
    /// EntityId / ApiKey / BaseUrl once credentials are obtained from HyperPay.
    ///
    /// HyperPay checkout flow:
    ///   1. POST {BaseUrl}/v1/checkouts  →  receive checkoutId
    ///   2. App renders hosted payment page: {BaseUrl}/paymentWidgets.js?checkoutId={id}
    ///   3. HyperPay posts result to your webhook (configure in HyperPay portal)
    ///   4. Backend queries GET {BaseUrl}/v1/checkouts/{id}/payment to verify
    /// </summary>
    public class MadaPaymentService : IMadaPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IReadFromAppSetting _config;
        private readonly ILogger<MadaPaymentService> _logger;

        public MadaPaymentService(
            HttpClient httpClient,
            IReadFromAppSetting config,
            ILogger<MadaPaymentService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<MadaCheckoutResult> InitiateCheckoutAsync(
            int orderId,
            decimal amount,
            string currency = "SAR",
            CancellationToken cancellationToken = default)
        {
            var isTestMode = _config.GetValue<bool>("MadaPayment:IsTestMode");

            if (isTestMode)
                return BuildTestModeResult(orderId, amount);

            return await CallHyperPayAsync(orderId, amount, currency, cancellationToken);
        }

        // ── Test mode ────────────────────────────────────────────────────────────────

        private MadaCheckoutResult BuildTestModeResult(int orderId, decimal amount)
        {
            var mockCheckoutId = $"TEST-{orderId}-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var baseUrl = _config.GetValue<string>("MadaPayment:BaseUrl")
                          ?? "https://eu-test.oppwa.com";

            _logger.LogInformation(
                "[TEST MODE] Mada checkout mock for order {OrderId}, amount {Amount} SAR. CheckoutId={CheckoutId}",
                orderId, amount, mockCheckoutId);

            return new MadaCheckoutResult
            {
                Success = true,
                CheckoutId = mockCheckoutId,
                CheckoutUrl = $"{baseUrl}/v1/paymentWidgets.js?checkoutId={mockCheckoutId}"
            };
        }

        // ── Live HyperPay call ────────────────────────────────────────────────────────

        private async Task<MadaCheckoutResult> CallHyperPayAsync(
            int orderId,
            decimal amount,
            string currency,
            CancellationToken cancellationToken)
        {
            try
            {
                var entityId = _config.GetValue<string>("MadaPayment:EntityId");
                var apiKey = _config.GetValue<string>("MadaPayment:ApiKey");
                var baseUrl = _config.GetValue<string>("MadaPayment:BaseUrl");

                if (string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("MadaPayment credentials are not configured.");
                    return Fail("Payment service is not configured.");
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                // HyperPay expects form-urlencoded
                var formData = new Dictionary<string, string>
                {
                    ["entityId"] = entityId,
                    ["amount"] = amount.ToString("F2"),
                    ["currency"] = currency,
                    ["paymentType"] = "DB",                     // Debit (direct charge)
                    ["merchantTransactionId"] = orderId.ToString()
                };

                using var content = new FormUrlEncodedContent(formData);
                var response = await _httpClient.PostAsync(
                    $"{baseUrl}/v1/checkouts", content, cancellationToken);

                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "HyperPay returned {StatusCode} for order {OrderId}. Body: {Body}",
                        response.StatusCode, orderId, body);
                    return Fail($"Payment gateway error ({(int)response.StatusCode}).");
                }

                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;

                if (!root.TryGetProperty("id", out var idElement))
                {
                    _logger.LogError(
                        "HyperPay response missing 'id' for order {OrderId}. Body: {Body}",
                        orderId, body);
                    return Fail("Unexpected response from payment gateway.");
                }

                var checkoutId = idElement.GetString() ?? string.Empty;
                return new MadaCheckoutResult
                {
                    Success = true,
                    CheckoutId = checkoutId,
                    CheckoutUrl = $"{baseUrl}/v1/paymentWidgets.js?checkoutId={checkoutId}"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error contacting HyperPay for order {OrderId}", orderId);
                return Fail("Could not reach payment gateway. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in MadaPaymentService for order {OrderId}", orderId);
                return Fail("An unexpected error occurred during payment processing.");
            }
        }

        private static MadaCheckoutResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}
