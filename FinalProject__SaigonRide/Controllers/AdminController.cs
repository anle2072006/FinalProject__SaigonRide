using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject__SaigonRide.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. Dashboard (Trang chủ Admin) ---
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            ViewBag.TotalStations = await _context.Stations.CountAsync();
            // Có thể đếm thêm coupon nếu cần:
            // ViewBag.TotalCoupons = await _context.Coupons.CountAsync();

            return View();
        }

        // --- 2. Quản lý Xe (Vehicles) ---
        public async Task<IActionResult> Vehicles()
        {
            var vehicles = await _context.Vehicles.ToListAsync();            // Trỏ về file View bạn đã có
            return View(vehicles);
        }

        // --- 3. Quản lý Trạm (Stations) ---
        public async Task<IActionResult> Stations()
        {
            var stations = await _context.Stations.ToListAsync();
            return View(stations); // Sẽ tìm file Views/Admin/Stations.cshtml
        }

        // --- 4. Quản lý Mã giảm giá (Coupons) ---
        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons.ToListAsync();
            return View(coupons); // Sẽ tìm file Views/Admin/Coupons.cshtml
        }

        // --- Các hàm xử lý dữ liệu (Ví dụ cho Vehicle) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVehicle(string Name, double PricePerHour, string StationId, int Quantity)
        {
            if (ModelState.IsValid)
            {
                if (Quantity <= 0) Quantity = 1;
                for (int i = 0; i < Quantity; i++)
                {
                    var vehicle = new Vehicle
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = Name,
                        PricePerHour = PricePerHour,
                        StationId = StationId
                    };
                    _context.Vehicles.Add(vehicle);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Vehicles));
            }
            return RedirectToAction(nameof(Vehicles));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVehicle(string id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Vehicles));
        }
    }
}