using FinalProject__SaigonRide.Controllers;
using FinalProject__SaigonRide.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class SettingController : Controller
{
    private readonly ApplicationDbContext _context;

    public SettingController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> IndexSetting()
    {
        // Lấy danh sách Coupon còn hiệu lực và còn số lượng
        var listCoupons = await _context.Coupons
            .Where(c => c.IsActive && c.ExpiryDate >= DateTime.Now && c.Quantity > 0)
            .ToListAsync();

        return View(listCoupons); // Truyền danh sách này sang View
    }
}
