using Microsoft.AspNetCore.Mvc;
using QLDH.Models;

namespace QLDH.Controllers
{
    public class DatabaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Connect(DatabaseConfig config)
        {
            if (ModelState.IsValid)
            {
                Database.Set_Database(
                    config.Host,
                    config.Port,
                    config.Sid,
                    config.User,
                    config.Password
                );

                bool success = Database.Connect();

                if (success)
                {
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.Message = "Kết nối Oracle thất bại!";
            }

            return View("Index");
        }
    }
}
