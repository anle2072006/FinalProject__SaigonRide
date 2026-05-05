using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;

namespace FinalProject__SaigonRide.Controllers
{
    public class InUseController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InUseController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexInUse()
        {
            // 1. Lấy danh sách Coupon còn hạn từ SQL Server
            var listCoupons = await _context.Coupons
                .Where(c => c.IsActive && c.Quantity > 0)
                .ToListAsync();

            // 2. Tạo Model và nạp dữ liệu vào
            var viewModel = new PaymentViewModel
            {
                EstimatedCost = 67777, // Số tiền bạn đang để ở View
                AvailableCoupons = listCoupons // Chuyền danh sách từ DB sang
            };

            // 3. QUAN TRỌNG: Phải truyền viewModel vào trong View()
            return View(viewModel);
        }
    }
}