using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FinalProject__SaigonRide.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Tạo một gói dữ liệu PaymentViewModel
            var paymentData = new FinalProject__SaigonRide.Models.PaymentViewModel();

            // Gắn sẵn một số tiền giả lập để test (sau này bạn sẽ lấy số này từ Database/Session)
            paymentData.EstimatedCost = 67677;

            // Gửi gói dữ liệu này sang cho file Index.cshtml
            return View(paymentData);
            //sau link voi may cai khac thi xoa het trong index
            //return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Payment()
        {
            var model = new PaymentViewModel();
            return View(model);
        }
    }
}