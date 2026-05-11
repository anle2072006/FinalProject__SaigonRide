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

            var activeBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse");

            if (activeBooking != null)
            {
                // Nếu có chuyến rồi, bay thẳng vào trang In-Use luôn
                return RedirectToAction("IndexInUse");
            }

            var newBooking = new Booking
            {
                UserId = user.Id,
                StationId = stationId,
                VehicleId = vehicleId,
                StartTime = DateTime.Now,
                Status = "InUse"
            };

            _context.Bookings.Add(newBooking);
            await _context.SaveChangesAsync();
            return RedirectToAction("IndexInUse");
        }

        public async Task<IActionResult> IndexInUse()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var currentBooking = await _context.Bookings
                .Include(b => b.Station)
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse");

            if (currentBooking == null) return RedirectToAction("Stations", "Home");

            var timeElapsed = DateTime.Now - currentBooking.StartTime;
            ViewBag.SecondsPassed = (int)timeElapsed.TotalSeconds;

            // --- CÁC DÒNG CODE CẦN THÊM ĐỂ FIX LỖI DROP-OFF ---

            // 1. Lấy danh sách trạm từ DB truyền sang ViewBag (Đổi .Stations thành tên DbSet của bạn nếu khác)
            ViewBag.StationList = await _context.Stations.ToListAsync();

            // 2. Kiểm tra xem user đã update trạm trả chưa
            ViewBag.HasDropOffStation = !string.IsNullOrEmpty(currentBooking.NextStationId);

            // 3. Lấy tên trạm Drop-off để truyền vào Payment Modal
            string dropOffName = "Not selected";
            if (!string.IsNullOrEmpty(currentBooking.NextStationId))
            {
                var dropOffStation = await _context.Stations.FindAsync(currentBooking.NextStationId);
                if (dropOffStation != null)
                {
                    dropOffName = dropOffStation.Name;
                }
            }

            // --------------------------------------------------

            var paymentModel = new PaymentViewModel
            {
                FirstName = user.FirstName ?? "Rider",
                LastName = user.LastName ?? "User",
                Email = user.Email ?? "No Email",
                Phone = user.PhoneNumber ?? "Not updated",
                PickupStation = currentBooking.Station?.Name ?? "Unknown Station",

                // 4. Gán tên trạm Drop-off vào Model
                DropoffStation = dropOffName,

                RentedVehicle = currentBooking.Vehicle?.Name ?? "Unknown Vehicle",
                VehicleImagePath = currentBooking.Vehicle?.ImagePath,
                AvailableCoupons = await _context.Coupons.Where(c => c.IsActive).ToListAsync(),
                PricePerMinute = (currentBooking.Vehicle?.PricePerHour ?? 0) / 60.0,
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