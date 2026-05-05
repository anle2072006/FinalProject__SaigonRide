using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject__SaigonRide.Models
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public double PricePerHour { get; set; }
        public string ImagePath { get; set; } = "";
    }
}