using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FinalProject__SaigonRide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            ViewBag.TotalStations = await _context.Stations.CountAsync();


            return View();
        }

        public async Task<IActionResult> Stations()
        {
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