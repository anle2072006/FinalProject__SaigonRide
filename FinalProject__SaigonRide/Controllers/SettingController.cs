using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Data; 
using FinalProject__SaigonRide.Models; 
using Microsoft.AspNetCore.Http; 
using System.Linq; 

namespace FinalProject__SaigonRide.Controllers
{
    public class SettingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult IndexSetting()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var userProfile = _context.AppUsers.FirstOrDefault(u => u.Email == userEmail);

            if (userProfile == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(userProfile);
        }
    }
}