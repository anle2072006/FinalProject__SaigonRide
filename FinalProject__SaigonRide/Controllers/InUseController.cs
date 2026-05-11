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

            // Chỉ dùng 1 lần logic tính thời gian này
            var endTime = (currentBooking.EndTime == DateTime.MinValue) ? DateTime.Now : currentBooking.EndTime; var timeElapsed = endTime - currentBooking.StartTime;
            ViewBag.SecondsPassed = (int)timeElapsed.TotalSeconds;
            ViewBag.IsFrozen = currentBooking.EndTime != DateTime.MinValue;
            ViewBag.StationList = await _context.Stations.ToListAsync();
            ViewBag.HasDropOffStation = !string.IsNullOrEmpty(currentBooking.NextStationId);

            string dropOffName = "Not selected";
            if (!string.IsNullOrEmpty(currentBooking.NextStationId))
            {
                var dropOffStation = await _context.Stations.FindAsync(currentBooking.NextStationId);
                if (dropOffStation != null)
                {
                    dropOffName = dropOffStation.Name;
                }
            }

            var paymentModel = new PaymentViewModel
            {
                FirstName = user.FirstName ?? "Rider",
                LastName = user.LastName ?? "User",
                Email = user.Email ?? "No Email",
                Phone = user.PhoneNumber ?? "Not updated",
                PickupStation = currentBooking.Station?.Name ?? "Unknown Station",
                DropoffStation = dropOffName,
                RentedVehicle = currentBooking.Vehicle?.Name ?? "Unknown Vehicle",
                VehicleImagePath = currentBooking.Vehicle?.ImagePath,
                AvailableCoupons = await _context.Coupons.Where(c => c.IsActive).ToListAsync(),
                PricePerMinute = currentBooking.Vehicle?.PricePerHour ?? 0,
                IsForeigner = user.IsForeigner,
                DocumentNumber = user.DocumentNumber ?? "Not updated",
            };

            return View(paymentModel);
        }

        // Hàm FreezeTrip phải nằm độc lập bên ngoài
        [HttpPost]
        public async Task<IActionResult> FreezeTrip()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var currentBooking = await _context.Bookings
.FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse" && b.EndTime == DateTime.MinValue);
            if (currentBooking != null)
            {
                currentBooking.EndTime = DateTime.Now;
                _context.Bookings.Update(currentBooking);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

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