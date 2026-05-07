using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using System.Linq;

namespace FinalProject__SaigonRide.Controllers
{
    public class InUseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InUseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Hàm nhận dữ liệu từ nút Book (Popup)
        [HttpPost]
        public IActionResult StartTrip(string stationId, string vehicleId)
        {
            // Lưu vào TempData để chuyển sang trang IndexInUse
            TempData["StationId"] = stationId;
            TempData["VehicleId"] = vehicleId;

            return RedirectToAction("IndexInUse");
        }

        // 2. Trang hiển thị thực tế
        public IActionResult IndexInUse()
        {
            // Lấy ID ra và dùng Keep để không bị mất dữ liệu khi F5 trang
            string sId = TempData["StationId"]?.ToString();
            string vId = TempData["VehicleId"]?.ToString();
            TempData.Keep();

            if (string.IsNullOrEmpty(sId) || string.IsNullOrEmpty(vId))
            {
                return RedirectToAction("IndexStations", "Stations");
            }

            // Lấy dữ liệu từ Database dựa trên ID
            var station = _context.Stations.FirstOrDefault(s => s.Id == sId);
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == vId);

            // Gán dữ liệu vào ViewBag để View (HTML) lấy ra dùng
            if (station != null)
            {
                ViewBag.StationId = station.Id;
                ViewBag.StationName = station.Name;
                ViewBag.StationLocation = station.Location;
                ViewBag.StationImg = station.ImagePath; // Link ảnh từ DB
            }

            if (vehicle != null)
            {
                ViewBag.VehicleId = vehicle.Id;
                ViewBag.VehicleName = vehicle.Name;
                ViewBag.VehicleImg = vehicle.ImagePath; // Link ảnh xe từ DB
            }

            return View();
        }
    }
}