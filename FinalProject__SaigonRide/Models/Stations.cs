using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinalProject__SaigonRide.Models
{
    public class Station
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";

        public string Location { get; set; } = "";

        public string ImagePath { get; set; } = "";
    }
}