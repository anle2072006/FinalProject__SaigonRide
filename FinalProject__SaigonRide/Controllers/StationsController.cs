using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace FinalProject__SaigonRide.Controllers
{
    public class StationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- GIAO DIỆN USER: HIỂN THỊ DANH SÁCH TRẠM ---
        public async Task<IActionResult> IndexStations()
        {
            var stations = await _context.Stations.ToListAsync();
            return View("~/Views/Stations/IndexStations.cshtml");
        }

        // --- GIAO DIỆN ADMIN: HIỂN THỊ FORM TẠO TRẠM ---
        public IActionResult Create()
        {
            return View();
        }

        // --- LÕI LOGIC: XỬ LÝ LƯU TRẠM VÀ TỰ TẠO XE ---
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Create(Station station)
        {
            // 1. Xóa ID khỏi bộ kiểm tra lỗi (để hệ thống không cản trở nếu ID rỗng)
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                // 2. ÉP BUỘC: Luôn luôn tạo một ID mới tinh và độc nhất cho Trạm
                station.Id = Guid.NewGuid().ToString();

                // 3. Thêm Trạm vào DB
                _context.Stations.Add(station);

                // 4. Tạo luôn 2 chiếc xe cho trạm đó
                var scooter = new Vehicle
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Electric Scooter",
                    PricePerHour = 1500,
                    StationId = station.Id
                };

                var bicycle = new Vehicle
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Bicycle",
                    PricePerHour = 500,
                    StationId = station.Id
                };

                _context.Vehicles.AddRange(scooter, bicycle);
                await _context.SaveChangesAsync();

                // Tùy bạn đang dùng hàm Index nào cho Admin thì Redirect về hàm đó
                return RedirectToAction(nameof(IndexStations));
            }
            return View(station);
        }
    }
}