using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FinalProject__SaigonRide.Controllers
{
    [Authorize]
    public class SettingController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return RedirectToAction("IndexSetting");
        }
        public async Task<IActionResult> IndexSetting()
        {
            // Lấy thông tin người dùng hiện tại trực tiếp từ Identity
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return Redirect("/Identity/Account/Login");

            return View(user);
        }
    }
}