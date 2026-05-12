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
            // Lấy danh sách trạm xe để truyền vào ViewBag cho modal "Add Vehicles"
            ViewBag.Stations = await _context.Stations.ToListAsync();

            var vehicles = await _context.Vehicles.ToListAsync();
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



        // --- 5. Xử lý Thêm Trạm (Create Station) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStation(string Name, string Location)
        {
            if (ModelState.IsValid)
            {
                var station = new Station
                {
                    Id = Guid.NewGuid().ToString(), // Tạo ID ngẫu nhiên vì Model của bạn dùng kiểu string
                    Name = Name,
                    Location = Location
                };

                _context.Stations.Add(station);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã thêm trạm mới thành công!";
                return RedirectToAction(nameof(Stations));
            }
            return RedirectToAction(nameof(Stations));
        }

        // --- 8. Xử lý Thêm Coupon (Create Coupon) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoupon(string CodeName, int DiscountValue, bool IsActive)
        {
            if (ModelState.IsValid)
            {
                var coupon = new Coupon
                {
                    // Nếu Id của Coupon là int tự tăng thì không cần dòng này. 
                    // Nếu là string như Station/Vehicle thì dùng Guid:
                    // Id = Guid.NewGuid().ToString(), 
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

        // --- 9. Xử lý Sửa Coupon (Edit Coupon) ---
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

        // --- 10. Xử lý Xóa Coupon (Delete Coupon) ---
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
        // --- Xử lý Sửa Trạm (Edit Station) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStation(string Id, string Name, string Location)
        {
            var station = await _context.Stations.FindAsync(Id);
            if (station != null)
            {
                station.Name = Name;
                station.Location = Location;

                _context.Stations.Update(station);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật thông tin trạm thành công!";
            }
            return RedirectToAction(nameof(Stations));
        }

        // --- Xử lý Xóa Trạm (Delete Station) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStation(string id) // Chữ id viết thường
        {
            var station = await _context.Stations.FindAsync(id);
            if (station != null)
            {
                // Lưu ý: Nếu trạm này ĐANG CÓ XE (Vehicles) thì không xóa được (lỗi khóa ngoại). 
                // Muốn xóa trạm, ông phải xóa hết xe trong trạm đó trước!
                _context.Stations.Remove(station);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa trạm thành công!";
            }
            return RedirectToAction(nameof(Stations));
        }

        // --- Xử lý Sửa Xe (Edit Vehicle) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(string Id, string Name, double PricePerHour, string StationId)
        {
            var vehicle = await _context.Vehicles.FindAsync(Id);
            if (vehicle != null)
            {
                vehicle.Name = Name;
                vehicle.PricePerHour = PricePerHour;
                vehicle.StationId = StationId;

                _context.Vehicles.Update(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật thông tin xe thành công!";
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