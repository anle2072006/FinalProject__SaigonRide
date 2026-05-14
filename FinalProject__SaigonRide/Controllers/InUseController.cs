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

        [HttpPost]
        public async Task<IActionResult> StartTrip(string stationId, string vehicleId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var activeBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse");

            if (activeBooking != null)
            {
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

            var endTime = (currentBooking.EndTime == DateTime.MinValue) ? DateTime.Now : currentBooking.EndTime;
            var timeElapsed = endTime - currentBooking.StartTime;

            ViewBag.SecondsPassed = (int)timeElapsed.TotalSeconds;
            ViewBag.IsFrozen = currentBooking.EndTime != DateTime.MinValue;

            ViewBag.StationList = await _context.Stations.ToListAsync();
            ViewBag.HasDropOffStation = !string.IsNullOrEmpty(currentBooking.NextStationId);

            string dropOffName = "Not selected";
            bool appliesDiscount = false; // Biến kiểm tra có được giảm giá trạm hay không

            if (!string.IsNullOrEmpty(currentBooking.NextStationId))
            {
                var dropOffStation = await _context.Stations.FindAsync(currentBooking.NextStationId);
                if (dropOffStation != null)
                {
                    dropOffName = dropOffStation.Name;

                    // KIỂM TRA SỨC CHỨA TRẠM ĐÍCH (< 20% THÌ ĐƯỢC GIẢM GIÁ)
                    if (dropOffStation.MaxCapacity > 0)
                    {
                        double capacityPercentage = ((double)dropOffStation.CurrentVehicles / dropOffStation.MaxCapacity) * 100;
                        if (capacityPercentage < 20)
                        {
                            appliesDiscount = true;
                        }
                    }
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
                DropoffStationId = currentBooking.NextStationId,

                // Gửi trạng thái giảm giá xuống Modal
                IsCapacityDiscount = appliesDiscount
            };

            var vehicleCountByStation = await _context.Vehicles
               .GroupBy(v => v.StationId)
               .Select(g => new { StationId = g.Key, Count = g.Count() })
               .ToDictionaryAsync(x => x.StationId, x => x.Count);

            ViewBag.VehicleCountByStation = vehicleCountByStation;
            ViewBag.MaxVehiclePerStation = 100;

            return View(paymentModel);
        }

        [HttpPost]
        public async Task<IActionResult> FreezeTrip()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var currentBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse" && b.EndTime == DateTime.MinValue);
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