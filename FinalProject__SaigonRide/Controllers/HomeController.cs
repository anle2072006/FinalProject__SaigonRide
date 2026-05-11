using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FinalProject__SaigonRide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // FIXED: Added userManager to parameters
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        [Route("/")]
        [Route("Dashboard")]
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            ViewBag.TotalStations = await _context.Stations.CountAsync();
            return View();
        }

        // FIXED: Only one Stations method remains
        public async Task<IActionResult> Stations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var activeBooking = await _context.Bookings
                    .AnyAsync(b => b.UserId == user.Id && b.Status == "InUse");

                if (activeBooking)
                {
                    TempData["AlreadyActive"] = true;
                }
            }

            var stations = await _context.Stations.ToListAsync();
            return View("~/Views/Stations/IndexStations.cshtml", stations);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Payment()
        {
            var model = new PaymentViewModel();
            return View(model);
        }
    }
}