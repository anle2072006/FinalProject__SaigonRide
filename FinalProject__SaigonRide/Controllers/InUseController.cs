using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;

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

        // 1. HÀM BẮT ĐẦU CHUYẾN ĐI (Lưu thẳng vào DB)
        [HttpPost]
        public async Task<IActionResult> StartTrip(string stationId, string vehicleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Kiểm tra xem User này có chuyến đi nào đang chạy dở không
            var activeBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse");

            if (activeBooking != null)
            {
                TempData["ErrorMessage"] = "You already have an active trip!";
                return RedirectToAction("IndexInUse");
            }

            // Tạo chuyến đi mới lưu vào DB
            var newBooking = new Booking
            {
                UserId = user.Id,
                StationId = stationId,
                VehicleId = vehicleId,
                StartTime = DateTime.Now,
                Status = "InUse" // Trạng thái bắt đầu chạy
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();

            return RedirectToAction("IndexInUse");
        }

        // 2. HÀM HIỂN THỊ GIAO DIỆN IN-USE
        public async Task<IActionResult> IndexInUse()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Lấy chuyến đi đang InUse của User từ DB
            var currentBooking = await _context.Bookings
                .Include(b => b.Station)
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse");

            // Nếu không có chuyến nào đang chạy -> Đuổi về trang chọn xe
            if (currentBooking == null)
            {
                TempData["NoTripError"] = "You have not booked any vehicle! Please select a vehicle to start your trip.";
                return RedirectToAction("Stations", "Home");
            }

            // Kiểm tra xem đã chọn trạm trả (NextStationId) hay chưa để truyền cờ sang View chặn nút End Ride
            ViewBag.HasDropOffStation = !string.IsNullOrEmpty(currentBooking.NextStationId);

            // Lấy danh sách trạm (loại trừ trạm đi) để hiển thị trong Modal chọn trạm trả
            ViewBag.StationList = await _context.Stations
                .Where(s => s.Id != currentBooking.StationId)
                .ToListAsync();

            var activeCoupons = await _context.Coupons.Where(c => c.IsActive).ToListAsync();

            // Nạp dữ liệu vào ViewModel
            var paymentModel = new PaymentViewModel
            {
                FirstName = user.FirstName ?? "Rider",
                LastName = user.LastName ?? "",
                Email = user.Email ?? "No Email",
                Phone = user.PhoneNumber ?? "Not updated",
                PickupStation = currentBooking.Station?.Name ?? "Unknown Station",
                DropoffStation = "Moving...",
                RentedVehicle = currentBooking.Vehicle?.Name ?? "Unknown Vehicle",
                VehicleImagePath = currentBooking.Vehicle?.ImagePath,
                AvailableCoupons = activeCoupons,
                EstimatedCost = 0,
                PricePerMinute = currentBooking.Vehicle?.PricePerHour ?? 0,
                IsForeigner = user.IsForeigner,
                DocumentNumber = user.DocumentNumber ?? "Not updated",
            };

            return View(paymentModel);
        }

        // 3. HÀM XỬ LÝ KHI NGƯỜI DÙNG CHỌN TRẠM TRẢ TỪ MODAL
        [HttpPost]
        public async Task<IActionResult> UpdateDropOff(string nextStationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var currentBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse");

            if (currentBooking != null && !string.IsNullOrEmpty(nextStationId))
            {
                currentBooking.NextStationId = nextStationId;
                _context.Bookings.Update(currentBooking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("IndexInUse");
        }
    }
}