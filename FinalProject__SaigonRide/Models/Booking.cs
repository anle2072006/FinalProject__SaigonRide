namespace FinalProject__SaigonRide.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";
        public int VehicleId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public double TotalPrice { get; set; }
    }
}