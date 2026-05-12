using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using FinalProject__SaigonRide.Services;
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
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<PaymentController> logger)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

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

                decimal amountInUsd = Math.Round((decimal)amountInVnd / 25000, 2);

                var clientId = _configuration["PayPal:ClientId"];
                var secret = _configuration["PayPal:Secret"];
                var returnUrl = _configuration["PayPal:ReturnUrl"];
                var cancelUrl = _configuration["PayPal:CancelUrl"];

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

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var orderPayload = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[] { new { amount = new { currency_code = "USD", value = amountInUsd.ToString(System.Globalization.CultureInfo.InvariantCulture) } } },
                    application_context = new { return_url = returnUrl, cancel_url = cancelUrl }
                };
                var orderResponse = await client.PostAsJsonAsync("https://api-m.sandbox.paypal.com/v2/checkout/orders", orderPayload);
                var orderData = await orderResponse.Content.ReadFromJsonAsync<JsonElement>();

                var links = orderData.GetProperty("links").EnumerateArray();
                string approveLink = links.FirstOrDefault(l => l.GetProperty("rel").GetString() == "approve").GetProperty("href").GetString();

                return Redirect(approveLink);
            }
            return Content("This payment method is currently under maintenance.");
        }

        public async Task<IActionResult> PaymentCallback()
        {
            var responseData = HttpContext.Request.Query;
            string vnp_ResponseCode = responseData["vnp_ResponseCode"];
            string vnp_TransactionNo = responseData["vnp_TransactionNo"];
            string vnp_Amount = responseData["vnp_Amount"];
            string vnp_OrderInfo = responseData["vnp_OrderInfo"];

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

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

                if (activeBooking != null)
                {
                    activeBooking.Status = "Completed";
                    activeBooking.EndTime = DateTime.Now;
                    activeBooking.TotalPrice = realAmount;
                    _context.Bookings.Update(activeBooking);
                }

                _context.TransactionHistories.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
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
            var secret = _configuration["PayPal:Secret"];
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var authBytes = Encoding.ASCII.GetBytes($"{clientId}:{secret}");
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://api-m.sandbox.paypal.com/v1/oauth2/token");
            tokenRequest.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
            var tokenResponse = await client.SendAsync(tokenRequest);
            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<JsonElement>();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenData.GetProperty("access_token").GetString());

            var captureResponse = await client.PostAsync($"https://api-m.sandbox.paypal.com/v2/checkout/orders/{token}/capture",
                new StringContent("", Encoding.UTF8, "application/json"));

            var captureData = await captureResponse.Content.ReadFromJsonAsync<JsonElement>();

            if (!captureResponse.IsSuccessStatusCode)
            {
                var errorJson = captureData.ToString();
                _logger.LogError($"PayPal Capture Failed: {errorJson}");
            }

            var activeBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.UserId == user.Id && b.Status == "InUse");
            var transaction = new TransactionHistory
            {
                UserId = user.Id,
                PaymentMethod = "PayPal",
                TransactionDate = DateTime.Now,
                TransactionNo = token,
                OrderDescription = "SaigonRide PayPal Payment"
            };

            if (captureResponse.IsSuccessStatusCode)
            {
                transaction.Status = "Success";
                long realAmount = 0;
                try
                {
                    if (captureData.TryGetProperty("purchase_units", out var units) && units.GetArrayLength() > 0)
                    {
                        var payments = units[0].GetProperty("payments");
                        if (payments.TryGetProperty("captures", out var captures) && captures.GetArrayLength() > 0)
                        {
                            var amountObj = captures[0].GetProperty("amount");
                            var amountUsdStr = amountObj.GetProperty("value").GetString();
                            if (decimal.TryParse(amountUsdStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amountUsd))
                            {
                                realAmount = (long)(amountUsd * 25000);
                            }
                        }
                    }
                }
                catch (Exception ex) { _logger.LogError("Lỗi JSON PayPal: " + ex.Message); }

                transaction.Amount = realAmount;

                if (activeBooking != null)
                {
                    activeBooking.Status = "Completed";
                    activeBooking.EndTime = DateTime.Now;
                    activeBooking.TotalPrice = (double)realAmount;
                    _context.Bookings.Update(activeBooking);
                }

                _context.TransactionHistories.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                transaction.Status = "Failed";
                transaction.Amount = 0;
                _context.TransactionHistories.Add(transaction);
                await _context.SaveChangesAsync();

                return RedirectToAction("IndexInUse", "InUse");
            }
        }
    }
}