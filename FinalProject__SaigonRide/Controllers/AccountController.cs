using FinalProject__SaigonRide.Data;
using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FinalProject__SaigonRide.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LUỒNG ĐĂNG KÝ (REGISTER)

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User newUser)
        {
            // Kiểm tra xem dữ liệu người dùng nhập có hợp lệ không
            if (ModelState.IsValid)
            {
                _context.AppUsers.Add(newUser);
                
                _context.SaveChanges();

                //  Lưu Session để ghi nhớ là ông này đã đăng nhập
                HttpContext.Session.SetString("UserEmail", newUser.Email);
                HttpContext.Session.SetString("UserName", newUser.Name);

                //  Chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Nếu dữ liệu nhập sai, trả lại đúng trang form đó kèm thông báo lỗi
            return View(newUser);
        }

        // LUỒNG ĐĂNG NHẬP (LOGIN)

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // 1. Tìm trong Database xem có ai khớp Email và Password này không
            var user = _context.AppUsers.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // 2. Nếu đăng nhập thành công -> Lưu Session để ghi nhớ mặt
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserName", user.Name);

                // 3. Chuyển hướng thẳng vào trang Setting (hoặc Dashboard)
                return RedirectToAction("Index", "Setting");
            }

            // Nếu không tìm thấy (sai email hoặc pass), báo lỗi ra giao diện
            ViewBag.ErrorMessage = "Email hoặc mật khẩu không chính xác!";
            return View();
        }

        // LUỒNG ĐĂNG XUẤT (LOGOUT)
        public IActionResult Logout()
        {
            // Xóa sạch toàn bộ trí nhớ (Session) của hệ thống về người này
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }
    }
}