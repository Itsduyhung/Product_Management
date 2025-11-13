using System;
using System.Collections.Generic;

namespace Products_Management.DTOs.Response
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public double TotalAmount { get; set; }
        public string? Status { get; set; }  // "Pending", "Paid", "Shipped", etc.
        public string? OrderCode { get; set; } // Mã đơn hàng PayOS
        public string? PaymentUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<OrderItemResponse> Items { get; set; } = new();
    }
}