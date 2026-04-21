using Microsoft.AspNetCore.Mvc;

namespace FinalProject__SaigonRide.Controllers
{
    public class StationsController : Controller
    {
        public IActionResult IndexStations()
        {
            return View();
        }
    }
}
