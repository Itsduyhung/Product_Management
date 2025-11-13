using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Products_Management.Service
{
    public class PayOSService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<PayOSService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // Trả về tuple (paymentUrl, transactionId)
        public async Task<(string PaymentUrl, string TransactionId)> CreatePaymentLinkAsync(
            string orderCode,
            double amount,
            string description)
        {
            _logger.LogInformation("CreatePaymentLinkAsync for order {OrderCode} amount {Amount}", orderCode, amount);

            // Choose base url (production -> sandbox fallback)
            string baseUrl = "https://api.payos.io"; // production hiện tại
            try
            {
                var addrs = await Dns.GetHostAddressesAsync("api.payos.io");
                if (addrs == null || addrs.Length == 0)
                {
                    baseUrl = "https://sandbox.payos.vn";
                    _logger.LogWarning("api.payos.io not resolvable, using sandbox");
                }
            }
            catch
            {
                baseUrl = "https://sandbox.payos.vn";
                _logger.LogWarning("DNS failed, using sandbox");
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);

            var clientId = _configuration["PayOSSettings:ClientId"];
            var apiKey = _configuration["PayOSSettings:ApiKey"];
            var checksumKey = _configuration["PayOSSettings:ChecksumKey"];
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(checksumKey))
                throw new Exception("Missing PayOS configuration");

            // Build payload that PayOS expects (camelCase)
            var payload = new
            {
                orderCode = orderCode,
                amount = amount,
                description = description,
                cancelUrl = $"{frontendUrl.TrimEnd('/')}/payment-cancel",
                returnUrl = $"{frontendUrl.TrimEnd('/')}/payment-success"
            };

            var jsonOpts = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(payload, jsonOpts);

            // signature HMAC-SHA256 over raw JSON (PayOS expects specific format — ensure this matches their doc)
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
            var signature = Convert.ToBase64String(signatureBytes);

            var finalRequest = new
            {
                orderCode = orderCode,
                amount = amount,
                description = description,
                cancelUrl = $"{frontendUrl.TrimEnd('/')}/payment-cancel",
                returnUrl = $"{frontendUrl.TrimEnd('/')}/payment-success",
                signature = signature
            };

            var requestBody = new StringContent(JsonSerializer.Serialize(finalRequest, jsonOpts), Encoding.UTF8, "application/json");

            _logger.LogDebug("POST {Base}/v2/payment-requests", client.BaseAddress);

            // NOTE: use v2/payment-requests or v1/payment/create depending on PayOS spec — here using v2
            var response = await client.PostAsync("/v2/payment-requests", requestBody);
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("PayOS response: {Response}", responseBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayOS returned error: {Status} - {Body}", response.StatusCode, responseBody);
                throw new Exception($"PayOS API error: {responseBody}");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // PayOS returns code "00" for success (or adjust based on their latest response)
            if (root.TryGetProperty("code", out var codeEl) && codeEl.GetString() != "00")
            {
                var msg = root.TryGetProperty("desc", out var descEl) ? descEl.GetString() : "Unknown PayOS error";
                throw new Exception($"PayOS API returned error: {msg}");
            }

            if (!root.TryGetProperty("data", out var dataEl))
                throw new Exception("PayOS response missing data");

            var checkoutUrl = dataEl.GetProperty("checkoutUrl").GetString() ?? "";
            var transactionId = dataEl.GetProperty("transactionId").GetString() ?? "";

            return (checkoutUrl, transactionId);
        }

        // Verify payment: call v2 endpoint to get status
        public async Task<JsonDocument> VerifyPaymentAsync(string orderCode)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.payos.io"); // or sandbox fallback as needed

            var clientId = _configuration["PayOSSettings:ClientId"];
            var apiKey = _configuration["PayOSSettings:ApiKey"];
            var checksumKey = _configuration["PayOSSettings:ChecksumKey"];
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(checksumKey))
                throw new Exception("Missing PayOS credentials");

            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var signData = $"{timestamp}|{apiKey}|{orderCode}";
            var signature = ComputeHmacSha256(signData, checksumKey);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-client-id", clientId);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("x-signature", signature);
            client.DefaultRequestHeaders.Add("x-timestamp", timestamp);

            var response = await client.GetAsync($"/v2/payment-requests/{orderCode}");
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("VerifyPaymentAsync failed: {Status} - {Body}", response.StatusCode, body);
                throw new Exception($"Verify failed: {body}");
            }

            return JsonDocument.Parse(body);
        }

        private string ComputeHmacSha256(string message, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hash);
        }
    }
}