using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Services;
using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

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
            if (paymentMethod == "PayPal")
            {
                string cleanAmount = amount.Replace("VND", "").Replace(".", "").Replace(",", "").Trim();
                if (!long.TryParse(cleanAmount, out long amountInVnd)) return BadRequest("Invalid amount.");

                // Quy đổi VND sang USD (Tỉ giá tạm tính: 25.000 VND = 1 USD)
                decimal amountInUsd = Math.Round((decimal)amountInVnd / 25000, 2);

                var clientId = _configuration["PayPal:ClientId"];
                var secret = _configuration["PayPal:Secret"];
                var returnUrl = _configuration["PayPal:ReturnUrl"];
                var cancelUrl = _configuration["PayPal:CancelUrl"];

                // 1. Lấy Access Token
                var authBytes = Encoding.ASCII.GetBytes($"{clientId}:{secret}");
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api-m.sandbox.paypal.com/v1/oauth2/token");
                tokenRequest.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var tokenResponse = await client.SendAsync(tokenRequest);
                var tokenData = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();

                if (tokenData.TryGetProperty("error", out var errorProp))
                {
                    string errorDesc = tokenData.TryGetProperty("error_description", out var descProp) ? descProp.GetString() : "Lỗi xác thực PayPal";
                    return BadRequest($"Chi tiết lỗi từ PayPal: {errorProp.GetString()} - {errorDesc}");
                }

                var accessToken = tokenData.GetProperty("access_token").GetString();

                // 2. Tạo Đơn Hàng (Order)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var orderPayload = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[] { new { amount = new { currency_code = "USD", value = amountInUsd.ToString(System.Globalization.CultureInfo.InvariantCulture) } } },
                    application_context = new { return_url = returnUrl, cancel_url = cancelUrl }
                };
                var orderResponse = await client.PostAsJsonAsync("https://api-m.sandbox.paypal.com/v2/checkout/orders", orderPayload);
                var orderData = await orderResponse.Content.ReadFromJsonAsync<JsonElement>();

                // 3. Link sang trang thanh toán của PayPal
                var links = orderData.GetProperty("links").EnumerateArray();
                string approveLink = links.FirstOrDefault(l => l.GetProperty("rel").GetString() == "approve").GetProperty("href").GetString();

                return Redirect(approveLink);
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

                return RedirectToAction("IndexInUse", "InUse");
            }
        }
        public async Task<IActionResult> PayPalCallback(string token)
        {
            var clientId = _configuration["PayPal:ClientId"];
            // ... (copy toàn bộ nội dung hàm PayPalCallback tôi đã gửi ở trên vào đây)

            return RedirectToAction("Index", "Dashboard");
        }
    }
}