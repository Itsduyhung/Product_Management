namespace Products_Management.DTOs.Response
{
    public class OrderItemResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }  // Lấy từ ProductEntity
        public int Quantity { get; set; }
        public double Price { get; set; }       // Giá đơn lẻ
        public double SubTotal => Price * Quantity; // Tự tính
    }
}