namespace Products_Management.DTOs.Response
{
    public class PayOsResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public object? data { get; set; }

        public PayOsResponse(int code, string message, object? data)
        {
            this.code = code;
            this.message = message;
            this.data = data;
        }
    }
}