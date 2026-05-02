namespace FinalProject__SaigonRide.Models 
{
    public class PaymentViewModel
    {
        public string FirstName { get; set; } = "Van A";
        public string LastName { get; set; } = "Nguyen";
        public string Email { get; set; } = "User@Gmail.Com";
        public string Phone { get; set; } = "0987654321";
        public string PickupStation { get; set; } = "Ben Thanh";
        public string DropoffStation { get; set; } = "Ba Son";
        public string RentedVehicle { get; set; } = "electric scooter";
        public decimal EstimatedCost { get; set; } = 67677;
        public decimal Discount { get; set; } = 15000;
        public decimal Total => EstimatedCost - Discount;
    }
}