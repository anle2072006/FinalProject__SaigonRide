using Microsoft.AspNetCore.Identity;

namespace FinalProject__SaigonRide.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}