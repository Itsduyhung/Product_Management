namespace Products_Management.DTOs.Request
{
    // PayOSWebhookRequest - không còn dùng, đã thay bằng PaymentCallbackRequest
    // Giữ lại để tránh breaking changes nếu có code khác đang dùng
    public class PayOSWebhookRequest
    {
        public PaymentCallbackRequest? Data { get; set; }
    }
}