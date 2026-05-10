using Microsoft.AspNetCore.Mvc;
using FinalProject__SaigonRide.Helpers;
using FinalProject__SaigonRide.Models;
using FinalProject__SaigonRide.Data;

namespace FinalProject__SaigonRide.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public PaymentController(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost]
        public IActionResult ProcessPayment(string amount, string paymentMethod)
        {
            if (paymentMethod == "VNPay")
            {
                return RedirectToAction("CreateVNPayPayment", new { amount = amount });
            }
            else if (paymentMethod == "PayPal")
            {
                // Sau này code PayPal bỏ vào đây
                return Content("Tính năng PayPal đang phát triển");
            }

            return RedirectToAction("IndexInUse", "InUse");
        }

        public IActionResult CreateVNPayPayment(string amount)
        {
            var vnpay = new VnpayLibrary();
            var vnp_Url = _configuration["Vnpay:BaseUrl"];
            var vnp_TmnCode = _configuration["Vnpay:TmnCode"];
            var vnp_HashSecret = _configuration["Vnpay:HashSecret"];
            var vnp_ReturnUrl = _configuration["Vnpay:ReturnUrl"];

            // Sửa lỗi parsing: Chỉ lấy số, bỏ chữ " VND" và dấu chấm
            string cleanAmount = amount.Replace(" VND", "").Replace(".", "").Replace(",", "").Trim();
            if (!long.TryParse(cleanAmount, out long amountInVnd) || amountInVnd <= 0)
            {
                return BadRequest("Số tiền không hợp lệ");
            }

            long finalAmount = amountInVnd * 100; // VNPay yêu cầu nhân 100

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", finalAmount.ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan SaigonRide");
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
            vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            var paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            return Redirect(paymentUrl);
        }

        public IActionResult PaymentCallback()
        {
            // Xử lý dữ liệu trả về từ VNPay (Success hay Fail) tại đây
            return Content("Thanh toán thành công! (Giả lập)");
        }
    }
}