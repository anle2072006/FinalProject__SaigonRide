using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject__SaigonRide.Controllers
{
    public class InUseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InUseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public IActionResult StartTrip(string stationId, string vehicleId)
        {
            TempData["StationId"] = stationId;
            TempData["VehicleId"] = vehicleId;
            return RedirectToAction("IndexInUse");
        }

        public async Task<IActionResult> IndexInUse()
        {
            string? sId = TempData["StationId"]?.ToString();
            string? vId = TempData["VehicleId"]?.ToString();
            TempData.Keep();

            if (string.IsNullOrEmpty(sId) || string.IsNullOrEmpty(vId))
            {
                return RedirectToAction("IndexStations", "Stations");
            }

            var station = await _context.Stations.FirstOrDefaultAsync(s => s.Id == sId);
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vId);

            var user = await _userManager.GetUserAsync(User);
            var activeCoupons = await _context.Coupons.Where(c => c.IsActive).ToListAsync();

            // CHỈ KHAI BÁO 1 LẦN DUY NHẤT: Đã gộp PricePerMinute vào chung "hộp" này
            var paymentModel = new PaymentViewModel
            {
                FirstName = user?.FirstName ?? "Rider",
                LastName = user?.LastName ?? "",
                Email = user?.Email ?? "No Email",
                Phone = user?.PhoneNumber ?? "Chưa cập nhật",
                PickupStation = station?.Name ?? "Unknown Station",
                DropoffStation = "Đang di chuyển...",
                RentedVehicle = vehicle?.Name ?? "Unknown Vehicle",
                AvailableCoupons = activeCoupons,
                EstimatedCost = 0,
                PricePerMinute = vehicle?.PricePerHour ?? 0 // Giá 500đ hoặc 1500đ lấy ở đây
            };

            if (station != null)
            {
                ViewBag.StationName = station.Name;
                ViewBag.StationImg = station.ImagePath;
            }
            if (vehicle != null)
            {
                ViewBag.VehicleName = vehicle.Name;
                ViewBag.VehicleImg = vehicle.ImagePath;
            }

            // CHỈ RETURN 1 LẦN Ở CUỐI HÀM
            return View(paymentModel);
        }
    }
}