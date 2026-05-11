using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Services;
using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject__SaigonRide.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }

        // 1. PROCESS PAYMENT (VNPay)
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string paymentMethod, string amount)
        {
            if (paymentMethod == "VNPay")
            {
                string cleanAmount = amount.Replace("VND", "").Replace(".", "").Replace(",", "").Trim();
                if (!long.TryParse(cleanAmount, out long amountInVnd))
                {
                    return BadRequest("Invalid amount.");
                }

                string tmnCode = _configuration["Vnpay:TmnCode"];
                string hashSecret = _configuration["Vnpay:HashSecret"];
                string vnpUrl = _configuration["Vnpay:BaseUrl"];
                string returnUrl = _configuration["Vnpay:ReturnUrl"];

                VnpayLibrary vnpay = new VnpayLibrary();
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", tmnCode);
                vnpay.AddRequestData("vnp_Amount", (amountInVnd * 100).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", "SaigonRide rental payment");
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
                vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

                string paymentUrl = vnpay.CreateRequestUrl(vnpUrl, hashSecret);
                return Redirect(paymentUrl);
            }

            return Content("This payment method is currently under maintenance.");
        }

        // 2. PAYMENT CALLBACK (Update Booking Status here)
        public async Task<IActionResult> PaymentCallback()
        {
            var responseData = HttpContext.Request.Query;
            string vnp_ResponseCode = responseData["vnp_ResponseCode"];
            string vnp_TransactionNo = responseData["vnp_TransactionNo"];
            string vnp_Amount = responseData["vnp_Amount"];
            string vnp_OrderInfo = responseData["vnp_OrderInfo"];

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // --- ADDED: Find the active booking for this user ---
            var activeBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse");

            var transaction = new TransactionHistory
            {
                UserId = user.Id,
                PaymentMethod = "VNPay",
                TransactionDate = DateTime.Now,
                TransactionNo = vnp_TransactionNo ?? "N/A",
                OrderDescription = vnp_OrderInfo ?? "SaigonRide Payment"
            };

            if (vnp_ResponseCode == "00")
            {
                long realAmount = 0;
                if (long.TryParse(vnp_Amount, out long amount))
                {
                    realAmount = amount / 100;
                }

                transaction.Amount = realAmount;
                transaction.Status = "Success";

                // --- ADDED: Update Booking to Completed ---
                if (activeBooking != null)
                {
                    activeBooking.Status = "Completed"; // Change status to stop the "Active Trip" check
                    activeBooking.EndTime = DateTime.Now; // Record the end time
                    activeBooking.TotalPrice = realAmount; // Save final price
                    _context.Bookings.Update(activeBooking);
                }

                _context.TransactionHistories.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                transaction.Amount = 0;
                transaction.Status = "Failed";
                _context.TransactionHistories.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Dashboard");
            }
        }
    }
}