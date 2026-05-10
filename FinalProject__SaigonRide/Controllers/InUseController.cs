using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 
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
            // 1. Dùng Session thay vì TempData để dữ liệu sống sót qua nhiều lần F5
            HttpContext.Session.SetString("TripStationId", stationId);
            HttpContext.Session.SetString("TripVehicleId", vehicleId);

            return RedirectToAction("IndexInUse");
        }

        public async Task<IActionResult> IndexInUse()
        {
            // 2. Rút dữ liệu chuyến đi từ Session
            string? sId = HttpContext.Session.GetString("TripStationId");
            string? vId = HttpContext.Session.GetString("TripVehicleId");

            if (string.IsNullOrEmpty(sId) || string.IsNullOrEmpty(vId))
            {
                return RedirectToAction("Stations", "Home");
            }
            // 3. LỚP BẢO VỆ: Nếu chưa có dữ liệu (người dùng chưa đặt xe)
            if (string.IsNullOrEmpty(sId) || string.IsNullOrEmpty(vId))
            {
                // Gửi một thông báo lỗi sang TempData để hiện Popup
                TempData["NoTripError"] = "Bạn chưa đặt phương tiện nào! Vui lòng chọn xe để bắt đầu chuyến đi.";

                // Đuổi về trang chọn Trạm xe
                return RedirectToAction("Stations", "Home");
            }

            // 4. Khối code lấy dữ liệu DB của bạn giữ nguyên
            var station = await _context.Stations.FirstOrDefaultAsync(s => s.Id == sId);
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vId);

            var user = await _userManager.GetUserAsync(User);
            var activeCoupons = await _context.Coupons.Where(c => c.IsActive).ToListAsync();

            var paymentModel = new PaymentViewModel
            {
                FirstName = user?.FirstName ?? "Rider",
                LastName = user?.LastName ?? "",
                Email = user?.Email ?? "No Email",
                Phone = user?.PhoneNumber ?? "Chưa cập nhật",
                PickupStation = station?.Name ?? "Unknown Station",
                DropoffStation = "Đang di chuyển...",
                RentedVehicle = vehicle?.Name ?? "Unknown Vehicle",
                VehicleImagePath = vehicle?.ImagePath,
                AvailableCoupons = activeCoupons,
                EstimatedCost = 0,
                PricePerMinute = vehicle?.PricePerHour ?? 0,
                IsForeigner = user?.IsForeigner ?? false,
                DocumentNumber = user?.DocumentNumber ?? "Not updated",
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

            return View(paymentModel);
        }
    }
}