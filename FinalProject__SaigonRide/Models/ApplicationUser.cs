using Microsoft.AspNetCore.Identity;

namespace FinalProject__SaigonRide.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsForeigner { get; set; }
        public string DocumentNumber { get; set; } 
    }
}