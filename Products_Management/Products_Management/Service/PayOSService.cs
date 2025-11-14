using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models;
using System.Text.Json;

namespace Products_Management.Service
{
    public class PayOSService
    {
        private readonly PayOSClient _payOSClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PayOSService> _logger;

        public PayOSService(
            IConfiguration configuration,
            ILogger<PayOSService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Initialize PayOS client using official SDK
            var clientId = _configuration["PayOSSettings:ClientId"];
            var apiKey = _configuration["PayOSSettings:ApiKey"];
            var checksumKey = _configuration["PayOSSettings:ChecksumKey"];

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(checksumKey))
                throw new Exception("Missing PayOS configuration");

            // Initialize PayOSClient with options - SDK will handle signature automatically
            var baseUrl = _configuration["PayOSSettings:BaseUrl"] ?? "https://api-merchant.payos.vn";
            
            _payOSClient = new PayOSClient(new PayOSOptions
            {
                ClientId = clientId,
                ApiKey = apiKey,
                ChecksumKey = checksumKey,
                BaseUrl = baseUrl,
                TimeoutMs = 30000,
                Logger = logger
            });

            _logger.LogInformation("PayOS SDK client initialized successfully with BaseUrl: {BaseUrl}", baseUrl);
        }

        // Trả về tuple (paymentUrl, transactionId)
        public async Task<(string PaymentUrl, string TransactionId)> CreatePaymentLinkAsync(
            string orderCode,
            double amount,
            string description)
        {
            _logger.LogInformation("CreatePaymentLinkAsync for order {OrderCode} amount {Amount}", orderCode, amount);

            try
            {
                var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:3000";
                var cancelUrl = $"{frontendUrl.TrimEnd('/')}/payment-cancel";
                var returnUrl = $"{frontendUrl.TrimEnd('/')}/payment-success";

                // Convert orderCode string to int (PayOS requires int)
                int orderCodeInt;
                if (!int.TryParse(orderCode, out orderCodeInt))
                {
                    // Use hash code as fallback to ensure it's always positive
                    orderCodeInt = Math.Abs(orderCode.GetHashCode());
                    _logger.LogWarning("OrderCode {OrderCode} is not numeric, using hash code: {HashCode}", orderCode, orderCodeInt);
                }

                // PayOS requires description to be max 25 characters
                // Truncate description if it exceeds 25 characters
                var truncatedDescription = description?.Length > 25 
                    ? description.Substring(0, 25) 
                    : description ?? "Payment";

                _logger.LogInformation("Description: '{Description}' (truncated from '{Original}')", 
                    truncatedDescription, description);

                // Create payment request using official SDK
                // SDK automatically handles signature calculation and headers
                // According to PayOS SDK v2.0.1 documentation, use PaymentRequests.CreateAsync()
                // Create CreatePaymentLinkRequest object matching SDK structure
                var paymentRequest = new PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest
                {
                    OrderCode = orderCodeInt,
                    Amount = (int)amount, // PayOS requires int for amount (in VND)
                    Description = truncatedDescription, // Max 25 characters
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                };

                _logger.LogInformation("Creating payment link with PayOS SDK - OrderCode: {OrderCode}, Amount: {Amount}, Description: {Description}", 
                    orderCodeInt, paymentRequest.Amount, description);

                // SDK automatically handles signature and headers
                // Use PaymentRequests.CreateAsync() from SDK (as per documentation)
                var paymentLinkResponse = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);

                _logger.LogInformation("Payment link created successfully - Response type: {Type}", 
                    paymentLinkResponse.GetType().Name);

                // Extract checkoutUrl and transactionId from response
                // SDK v2.0.1 may have different property structure
                // Serialize to JSON to inspect structure
                var responseJson = JsonSerializer.Serialize(paymentLinkResponse);
                _logger.LogInformation("Payment link response JSON: {Json}", responseJson);

                // Parse JSON to extract checkoutUrl and transactionId
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string checkoutUrl = "";
                string transactionId = "";

                // Try to extract from response structure
                if (root.TryGetProperty("data", out var dataEl))
                {
                    checkoutUrl = dataEl.TryGetProperty("checkoutUrl", out var urlEl) ? urlEl.GetString() ?? "" : "";
                    checkoutUrl = checkoutUrl == "" && dataEl.TryGetProperty("CheckoutUrl", out var urlEl2) ? urlEl2.GetString() ?? "" : checkoutUrl;
                    
                    transactionId = dataEl.TryGetProperty("transactionId", out var txnEl) ? txnEl.GetString() ?? "" : "";
                    transactionId = transactionId == "" && dataEl.TryGetProperty("TransactionId", out var txnEl2) ? txnEl2.GetString() ?? "" : transactionId;
                }
                else if (root.TryGetProperty("checkoutUrl", out var directUrlEl))
                {
                    checkoutUrl = directUrlEl.GetString() ?? "";
                    transactionId = root.TryGetProperty("transactionId", out var directTxnEl) ? directTxnEl.GetString() ?? "" : "";
                }

                return (checkoutUrl, transactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayOS API error - Message: {Message}, Type: {Type}", 
                    ex.Message, ex.GetType().Name);
                
                // Check if it's a PayOS-specific exception
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner: {ex.InnerException.Message}";
                }
                
                throw new Exception($"PayOS API error: {errorMessage}");
            }
        }

        // Verify payment: call v2 endpoint to get status
        public async Task<JsonDocument> VerifyPaymentAsync(string orderCode)
        {
            try
            {
                // Convert orderCode string to int
                int orderCodeInt;
                if (!int.TryParse(orderCode, out orderCodeInt))
                {
                    orderCodeInt = Math.Abs(orderCode.GetHashCode());
                }

                _logger.LogInformation("Verifying payment for orderCode: {OrderCode}", orderCodeInt);

                // Use SDK to get payment request details
                // SDK automatically handles signature and headers
                // Use PaymentRequests.GetAsync() from SDK
                var paymentLink = await _payOSClient.PaymentRequests.GetAsync(orderCodeInt);

                _logger.LogInformation("Payment verified - Response type: {Type}", paymentLink.GetType().Name);

                // Serialize to JSON to inspect structure
                var responseJson = JsonSerializer.Serialize(paymentLink);
                _logger.LogInformation("Payment verification response JSON: {Json}", responseJson);

                // Convert PaymentLink to JsonDocument for compatibility
                // Wrap in standard response format
                var json = JsonSerializer.Serialize(new
                {
                    code = "00",
                    desc = "Success",
                    data = paymentLink
                });

                _logger.LogInformation("Payment verified successfully for orderCode: {OrderCode}", orderCodeInt);

                return JsonDocument.Parse(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayOS API error verifying payment - Message: {Message}, Type: {Type}", 
                    ex.Message, ex.GetType().Name);
                
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner: {ex.InnerException.Message}";
                }
                
                throw new Exception($"Failed to verify payment: {errorMessage}");
            }
        }
    }
}
