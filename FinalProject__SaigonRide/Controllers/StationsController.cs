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

        public async Task<IActionResult> IndexStations()
        {
            var stations = await _context.Stations.ToListAsync();
            return View("~/Views/Stations/IndexStations.cshtml", stations); // Đã truyền data vào View
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Station station)
        {
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                station.Id = Guid.NewGuid().ToString();

                // Mặc định lúc mới tạo, trạm chưa có xe nào
                station.CurrentVehicles = 0;

                _context.Stations.Add(station);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(IndexStations));
            }
            return View(station);
        }
    }
}