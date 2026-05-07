using System.ComponentModel.DataAnnotations;

namespace FinalProject__SaigonRide.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CodeName { get; set; }

        public int DiscountValue { get; set; }

        public bool IsActive { get; set; } = true;
    }
}