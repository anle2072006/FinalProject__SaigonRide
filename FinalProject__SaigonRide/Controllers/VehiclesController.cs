using Microsoft.AspNetCore.Mvc;

namespace FinalProject__SaigonRide.Controllers
{
    public class VehiclesController : Controller
    {
        // Thêm tham số stationName
        public IActionResult IndexVehicles(string stationId, string stationName)
        {
            ViewBag.StationId = stationId;
            // Cất thêm cái tên trạm vào ViewBag
            ViewBag.StationName = stationName;

            return View();
        }
    }
}