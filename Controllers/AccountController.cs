using Microsoft.AspNetCore.Mvc;
using QLDH.Models;
using QLDH.Services;
using System.Text;
using System.Security.Cryptography;

namespace QLDH.Controllers
{
    public class AccountController : Controller
    {
        private readonly OracleAuthService _auth;
        private readonly CryptoService _crypto;
        private readonly OracleSecurityService _security;

        public AccountController(OracleAuthService auth, CryptoService crypto, OracleSecurityService security)
        {
            _auth = auth;
            _crypto = crypto;
            _security = security;
        }

        // ----------------- LOGIN -----------------
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = _auth.GetTaiKhoanByMaNV(model.Manv);
            if (account == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(model);
            }

            try
            {
                // ✅ Giải mã AES mật khẩu trong DB
                string decrypted = _crypto.Decrypt(account.MATKHAU);

                // ✅ So sánh an toàn (tránh timing attack)
                var a = Encoding.UTF8.GetBytes(decrypted);
                var b = Encoding.UTF8.GetBytes(model.Matkhau);

                if (a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b))
                {
                    // Đăng nhập thành công - Lưu session
                    HttpContext.Session.SetString("MaNV", account.MANV);
                    HttpContext.Session.SetString("VaiTro", account.VAITRO);
                    
                    // Lấy tên nhân viên để hiển thị
                    var tenNV = _auth.GetTenNVByMaNV(account.MANV);
                    HttpContext.Session.SetString("TenNV", tenNV);

                    // ✅ Set VPD context trong Oracle (cho VPD policy)
                    _security.SetUserContext(account.MANV, account.VAITRO);

                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Mật khẩu không đúng.");
                    return View(model);
                }
            }
            catch (CryptographicException)
            {
                ModelState.AddModelError("", "Lỗi giải mã (khả năng khóa sai hoặc dữ liệu bị chỉnh sửa).");
                return View(model);
            }
        }

        // ----------------- REGISTER -----------------
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = _auth.GetTaiKhoanByMaNV(model.Manv);
            if (existing != null)
            {
                ModelState.AddModelError("", "Mã nhân viên đã tồn tại.");
                return View(model);
            }

            // Xác định vai trò tự động
            string vaiTro = model.ChucVu switch
            {
                "Bán hàng" => "Bán hàng",
                "Kho hàng" => "Kho hàng",
                _ => "User"
            };

            // ✅ Mã hoá AES mật khẩu trước khi lưu
            string encryptedPassword = _crypto.Encrypt(model.Matkhau);

            // Tạo NHÂN VIÊN và TÀI KHOẢN
            _auth.CreateNhanVien(model.Manv, model.Tennv, model.ChucVu, model.DiaChi, model.Sdt, model.Email);
            _auth.CreateTaiKhoan(model.Manv, encryptedPassword, vaiTro);

            // ✅ Mã hóa DIACHI và SDT ở Database-level (gọi Oracle procedure)
            _security.EncryptNhanVienData(model.Manv);

            TempData["SuccessMessage"] = "Đăng ký thành công!";
            return RedirectToAction("Login");
        }

        // ----------------- DASHBOARD -----------------
        public IActionResult Dashboard() => View();

        // ----------------- LOGOUT -----------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ----------------- CHANGE PASSWORD -----------------
        [HttpGet]
        public IActionResult ChangePass() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePass(ChangePassViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var account = _auth.GetTaiKhoanByMaNV(model.Manv);
            if (account == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(model);
            }

            if (model.MatkhauMoi != model.NhapLaiMatkhauMoi)
            {
                ModelState.AddModelError("", "Mật khẩu mới nhập lại không khớp.");
                return View(model);
            }

            // ✅ Mã hoá AES mật khẩu mới
            string encryptedNewPass = _crypto.Encrypt(model.MatkhauMoi);
            _auth.UpdatePassword(model.Manv, encryptedNewPass);

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Login");
        }

        // ----------------- CHANGE PASSWORD (INSIDE DASHBOARD) -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassLogged(string MatkhauMoi, string NhapLaiMatkhauMoi)
        {
            if (MatkhauMoi != NhapLaiMatkhauMoi)
            {
                TempData["ErrorMessage"] = "Mật khẩu nhập lại không khớp.";
                return RedirectToAction("Dashboard");
            }

            var manv = HttpContext.Session.GetString("MaNV");
            if (string.IsNullOrEmpty(manv))
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login");
            }

            // ✅ Mã hoá AES mật khẩu mới
            string encryptedNewPass = _crypto.Encrypt(MatkhauMoi);
            _auth.UpdatePassword(manv, encryptedNewPass);

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Dashboard");
        }
    }
}
