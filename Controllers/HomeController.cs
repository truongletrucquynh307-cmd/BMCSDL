using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QLDH.Authorization;
using QLDH.Models;

namespace QLDH.Controllers
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
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Action hiển thị khi không có quyền truy cập
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.MaNV = HttpContext.Session.GetString("MaNV");
            ViewBag.VaiTro = HttpContext.Session.GetString("VaiTro");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
