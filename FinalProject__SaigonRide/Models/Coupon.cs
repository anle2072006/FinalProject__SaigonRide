using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Coupon
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } // Ví dụ: SAIGON2026

    [Required]
    [Column(TypeName = "decimal(18,2)")] // Ép kiểu decimal trong SQL để không bị lỗi tiền tệ
    public decimal DiscountValue { get; set; } // Số tiền giảm (10000, 20000...)

    public string? Description { get; set; } // Mô tả mã này dùng cho ai

    public DateTime ExpiryDate { get; set; } // Ngày hết hạn mã

    public bool IsActive { get; set; } = true; // Còn hiệu lực hay không

    public int Quantity { get; set; } // Số lần còn lại có thể sử dụng
}