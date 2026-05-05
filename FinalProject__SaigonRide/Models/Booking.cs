namespace FinalProject__SaigonRide.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public string UserId { get; set; } = "";

        public string VehicleId { get; set; } = "";

        public string StationId { get; set; } = "";

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public double TotalPrice { get; set; }

        public virtual Vehicle? Vehicle { get; set; }
        public virtual Station? Station { get; set; }
    }
}