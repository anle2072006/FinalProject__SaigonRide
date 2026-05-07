using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;

namespace FinalProject__SaigonRide.Controllers
{
    // [Authorize(Roles = "Admin")] // Bỏ comment nếu muốn bảo vệ bằng role
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================== DASHBOARD =====================
        public async Task<IActionResult> Index()
        {
            ViewBag.VehicleCount  = await _context.Vehicles.CountAsync();
            ViewBag.StationCount  = await _context.Stations.CountAsync();
            ViewBag.BookingCount  = await _context.Bookings.CountAsync();
            return View();
        }

        // ===================== VEHICLES =====================

        // GET: Admin/Vehicles
        public async Task<IActionResult> Vehicles()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            return View(vehicles);
        }

        // POST: Admin/CreateVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVehicle(string Name, double PricePerHour)
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                _context.Vehicles.Add(new Vehicle { Name = Name, PricePerHour = PricePerHour });
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Vehicle \"{Name}\" đã được thêm thành công!";
            }
            return RedirectToAction(nameof(Vehicles));
        }

        // GET: Admin/GetVehicle/5  (AJAX – trả JSON cho modal Edit)
        [HttpGet]
        public async Task<IActionResult> GetVehicle(int id)
        {
            var v = await _context.Vehicles.FindAsync(id);
            if (v == null) return NotFound();
            return Json(new { v.Id, v.Name, v.PricePerHour });
        }

        // POST: Admin/EditVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(int Id, string Name, double PricePerHour)
        {
            var v = await _context.Vehicles.FindAsync(Id);
            if (v != null)
            {
                v.Name          = Name;
                v.PricePerHour  = PricePerHour;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Vehicle \"{Name}\" đã được cập nhật!";
            }
            return RedirectToAction(nameof(Vehicles));
        }

        // POST: Admin/DeleteVehicle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var v = await _context.Vehicles.FindAsync(id);
            if (v != null)
            {
                _context.Vehicles.Remove(v);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Vehicle \"{v.Name}\" đã được xóa!";
            }
            return RedirectToAction(nameof(Vehicles));
        }

        // ===================== STATIONS =====================

        // GET: Admin/Stations
        public async Task<IActionResult> Stations()
        {
            var stations = await _context.Stations.ToListAsync();
            return View(stations);
        }

        // POST: Admin/CreateStation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStation(string Name, string Location)
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                _context.Stations.Add(new Station { Name = Name, Location = Location });
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Station \"{Name}\" đã được thêm thành công!";
            }
            return RedirectToAction(nameof(Stations));
        }
        // ===================== COUPONS =====================

        // GET: Admin/Coupons
        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons.ToListAsync();
            return View(coupons);
        }

        // POST: Admin/CreateCoupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoupon(string CodeName, int DiscountValue, bool IsActive = false)
        {
            if (!string.IsNullOrWhiteSpace(CodeName))
            {
                _context.Coupons.Add(new Coupon { CodeName = CodeName, DiscountValue = DiscountValue, IsActive = IsActive });
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Coupon \"{CodeName}\" đã được thêm!";
            }
            return RedirectToAction(nameof(Coupons));
        }

        // GET: Admin/GetCoupon/5  (AJAX – trả JSON cho modal Edit)
        [HttpGet]
        public async Task<IActionResult> GetCoupon(int id)
        {
            var c = await _context.Coupons.FindAsync(id);
            if (c == null) return NotFound();
            return Json(new { c.Id, c.CodeName, c.DiscountValue, c.IsActive });
        }

        // POST: Admin/EditCoupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoupon(int Id, string CodeName, int DiscountValue, bool IsActive = false)
        {
            var c = await _context.Coupons.FindAsync(Id);
            if (c != null)
            {
                c.CodeName = CodeName;
                c.DiscountValue = DiscountValue;
                c.IsActive = IsActive;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Coupon \"{CodeName}\" đã được cập nhật!";
            }
            return RedirectToAction(nameof(Coupons));
        }

        // POST: Admin/DeleteCoupon/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var c = await _context.Coupons.FindAsync(id);
            if (c != null)
            {
                _context.Coupons.Remove(c);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Coupon \"{c.CodeName}\" đã được xóa!";
            }
            return RedirectToAction(nameof(Coupons));
        }
        // GET: Admin/GetStation/5  (AJAX – trả JSON cho modal Edit)
        [HttpGet]
        public async Task<IActionResult> GetStation(int id)
        {
            var s = await _context.Stations.FindAsync(id);
            if (s == null) return NotFound();
            return Json(new { s.Id, s.Name, s.Location });
        }

        // POST: Admin/EditStation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStation(int Id, string Name, string Location)
        {
            var s = await _context.Stations.FindAsync(Id);
            if (s != null)
            {
                s.Name     = Name;
                s.Location = Location;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Station \"{Name}\" đã được cập nhật!";
            }
            return RedirectToAction(nameof(Stations));
        }

        // POST: Admin/DeleteStation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStation(int id)
        {
            var s = await _context.Stations.FindAsync(id);
            if (s != null)
            {
                _context.Stations.Remove(s);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Station \"{s.Name}\" đã được xóa!";
            }
            return RedirectToAction(nameof(Stations));
        }
    }
}
