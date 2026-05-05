using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Models; 
namespace FinalProject__SaigonRide.Controllers
{
    public class InUseController : Controller
    {
        [HttpPost]
        public IActionResult StartTrip(string stationId, string vehicleId)
        {
            return RedirectToAction("IndexInUse");
        }

        public IActionResult IndexInUse()
        {
            // Tạo một model rỗng để cung cấp "chất liệu" cho trang View và cái Modal
            var paymentModel = new PaymentViewModel();

            // Truyền model đó sang View
            return View(paymentModel);
        }
    }
}