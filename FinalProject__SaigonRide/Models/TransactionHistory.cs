using System.ComponentModel.DataAnnotations;

namespace FinalProject__SaigonRide.Models
{
    public class TransactionHistory
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public long Amount { get; set; }
        public string PaymentMethod { get; set; } 
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; } 
        public string TransactionNo { get; set; } 
        public string OrderDescription { get; set; }
    }
}