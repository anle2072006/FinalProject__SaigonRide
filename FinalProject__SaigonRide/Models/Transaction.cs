using System;
using System.ComponentModel.DataAnnotations;

namespace FinalProject__SaigonRide.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public string OrderId { get; set; } // Mã đơn hàng
        public string TransactionId { get; set; } // Mã giao dịch VNPay
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "VNPay";
        public string Status { get; set; } // "Success" hoặc "Failed"
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string UserId { get; set; } // Link tới người dùng nếu cần
    }
}