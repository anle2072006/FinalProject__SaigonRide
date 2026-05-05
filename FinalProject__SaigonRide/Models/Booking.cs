using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Thêm dòng này

namespace FinalProject__SaigonRide.Models
{
    public class Booking
    {
        [Key]
        public string Id { get; set; } = "";

        public string? VehicleId { get; set; }
        public string? StationId { get; set; }
        public DateTime BookingDate { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Fix lỗi warning ở đây
        public decimal TotalCost { get; set; }
    }
}