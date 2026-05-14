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

        // --- 1. Dashboard ---
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalVehicles = await _context.Vehicles.CountAsync();
            ViewBag.TotalStations = await _context.Stations.CountAsync();
            return View();
        }

        // --- 2. Vehicles ---
        public async Task<IActionResult> Vehicles()
        {
            ViewBag.Stations = await _context.Stations.ToListAsync();
            var vehicles = await _context.Vehicles.ToListAsync();
            return View(vehicles);
        }

        // --- 3. Stations ---
        public async Task<IActionResult> Stations()
        {
            var stations = await _context.Stations.ToListAsync();

            var vehicleCountByStation = await _context.Vehicles
                .GroupBy(v => v.StationId)
                .Select(g => new { StationId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StationId, x => x.Count);

            ViewBag.VehicleCountByStation = vehicleCountByStation;
            ViewBag.MaxVehiclePerStation = 100;

            return View(stations);
        }

        // --- 4. Coupons ---
        public async Task<IActionResult> Coupons()
        {
            var coupons = await _context.Coupons.ToListAsync();
            return View(coupons);
        }

        // --- 5. Create Station ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStation(string Name, string Location, int MaxCapacity)
        {
            if (ModelState.IsValid)
            {
                var station = new Station
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = Name,
                    Location = Location,
                    MaxCapacity = MaxCapacity,
                    CurrentVehicles = 0 // Mặc định khi mới tạo là 0 xe
                };
                _context.Stations.Add(station);
                await _context.SaveChangesAsync();
                TempData["Success"] = "New station added successfully!";
                return RedirectToAction(nameof(Stations));
            }
            return RedirectToAction(nameof(Stations));
        }

        // --- 6. Edit Station ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStation(string Id, string Name, string Location, int MaxCapacity)
        {
            var station = await _context.Stations.FindAsync(Id);
            if (station != null)
            {
                station.Name = Name;
                station.Location = Location;
                station.MaxCapacity = MaxCapacity;

                _context.Stations.Update(station);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin trạm thành công!";
            }
            return RedirectToAction(nameof(Stations));
        }

        // --- 7. Delete Station ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStation(string id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station != null)
            {
                _context.Stations.Remove(station);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa trạm thành công!";
            }
            return RedirectToAction(nameof(Stations));
        }

        // --- 8. Create Coupon ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoupon(string CodeName, int DiscountValue, bool IsActive)
        {
            if (ModelState.IsValid)
            {
                var coupon = new Coupon
                {
                    CodeName = CodeName,
                    DiscountValue = DiscountValue,
                    IsActive = IsActive
                };
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã tạo mã giảm giá mới!";
            }
            return RedirectToAction(nameof(Coupons));
        }

        // --- 9. Edit Coupon ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoupon(int Id, string CodeName, int DiscountValue, bool IsActive)
        {
            var coupon = await _context.Coupons.FindAsync(Id);
            if (coupon != null)
            {
                coupon.CodeName = CodeName;
                coupon.DiscountValue = DiscountValue;
                coupon.IsActive = IsActive;
                _context.Update(coupon);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật mã giảm giá!";
            }
            return RedirectToAction(nameof(Coupons));
        }

        // --- 10. Delete Coupon ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa mã giảm giá!";
            }
            return RedirectToAction(nameof(Coupons));
        }

        // --- 11. Create Vehicle ---
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

                // Tăng CurrentVehicles của Trạm
                var station = await _context.Stations.FindAsync(StationId);
                if (station != null)
                {
                    station.CurrentVehicles += Quantity;
                    _context.Stations.Update(station);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehicle added successfully!";
                return RedirectToAction(nameof(Vehicles));
            }
            return RedirectToAction(nameof(Vehicles));
        }

        // --- 12. Edit Vehicle ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(string Id, string Name, double PricePerHour, string StationId)
        {
            var vehicle = await _context.Vehicles.FindAsync(Id);
            if (vehicle != null)
            {
                // Nếu xe bị đổi sang trạm khác, cần tính toán lại CurrentVehicles của cả 2 trạm
                if (vehicle.StationId != StationId)
                {
                    var oldStation = await _context.Stations.FindAsync(vehicle.StationId);
                    if (oldStation != null && oldStation.CurrentVehicles > 0)
                    {
                        oldStation.CurrentVehicles -= 1;
                        _context.Stations.Update(oldStation);
                    }

                    var newStation = await _context.Stations.FindAsync(StationId);
                    if (newStation != null)
                    {
                        newStation.CurrentVehicles += 1;
                        _context.Stations.Update(newStation);
                    }
                }

                vehicle.Name = Name;
                vehicle.PricePerHour = PricePerHour;
                vehicle.StationId = StationId;
                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật thông tin xe thành công!";
            }
            return RedirectToAction(nameof(Vehicles));
        }

        // --- 13. Delete Vehicle ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVehicle(string id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                // Giảm CurrentVehicles của Trạm
                var station = await _context.Stations.FindAsync(vehicle.StationId);
                if (station != null && station.CurrentVehicles > 0)
                {
                    station.CurrentVehicles -= 1;
                    _context.Stations.Update(station);
                }

                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa xe thành công!";
            }
            return RedirectToAction(nameof(Vehicles));
        }
    }
}