using Net.payOS.Types;

namespace Products_Management.DTOs.Request
{
    public class PayOSWebhookRequest
    {
        public WebhookType Data { get; set; } // dùng kiểu của PayOS SDK
    }
}