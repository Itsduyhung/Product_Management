namespace Products_Management.Model
{
    public class OrderEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double TotalAmount { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        // 🧾 PayOS info
        public string? OrderCode { get; set; } // Mã đơn hàng dùng để map với PayOS
        public string? PaymentLink { get; set; } // Link thanh toán PayOS
        public string? TransactionId { get; set; } // Mã giao dịch thực tế khi thanh toán xong
        public DateTime? PaidAt { get; set; } // Thời điểm thanh toán
        public ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();
    }
}