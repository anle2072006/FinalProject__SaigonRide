using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject__SaigonRide.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexVehicles(string stationId, string stationName)
        {
            // Cất ID và Tên trạm vào ViewBag để dùng cho Modal
            ViewBag.StationId = stationId;
            ViewBag.StationName = stationName;

            // Truy vấn Database: Tìm tất cả xe có StationId bằng với ID trạm vừa chọn
            var vehicles = await _context.Vehicles
                                         .Where(v => v.StationId == stationId)
                                         .ToListAsync();

            // Trả danh sách xe thật này ra giao diện
            return View(vehicles);
        }
    }
}