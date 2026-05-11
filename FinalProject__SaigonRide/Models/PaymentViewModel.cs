using FinalProject__SaigonRide.Models;
using System.Collections.Generic;

namespace FinalProject__SaigonRide.Models
{
    public class PaymentViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PickupStation { get; set; }
        public string DropoffStation { get; set; } = "Ba Son";
        public string RentedVehicle { get; set; }
        public string VehicleImagePath { get; set; }
        public decimal EstimatedCost { get; set; }
        public bool IsForeigner { get; set; }
        public string DocumentNumber { get; set; }
        public double PricePerMinute { get; set; }
        public List<Coupon> AvailableCoupons { get; set; } = new List<Coupon>();
    }
}