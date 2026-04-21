using Microsoft.AspNetCore.Mvc;

namespace FinalProject__SaigonRide.Controllers
{
    public class VehiclesController : Controller
    {
        public IActionResult IndexVehicles()
        {
            return View();
        }
    }
}
