using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDH.Authorization;
using QLDH.Models;
using QLDH.Services.Encryption;
using System.Linq;
using System.Threading.Tasks;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CustomerEncryptionService _encryptionService;
        private readonly AuthorizationService _authService;

        public KhachHangController(
            ApplicationDbContext context,
            CustomerEncryptionService encryptionService,
            AuthorizationService authService)
        {
            _context = context;
            _encryptionService = encryptionService;
            _authService = authService;
        }

        // GET: Danh sách khách hàng
        [PermissionAuthorize(Permissions.KhachHang_View)]
        public IActionResult IndexKH()
        {
            var dsKhachHang = _context.KhachHang
                 .OrderBy(kh => Convert.ToInt32(kh.MAKH.Substring(2)))
                 .ToList();

            // Decrypt dữ liệu trước khi hiển thị
            foreach (var kh in dsKhachHang)
            {
                try
                {
                    kh.TENKH = _encryptionService.DecryptTenKH(kh.TENKH);
                    kh.DIACHI = _encryptionService.DecryptDiaChi(kh.DIACHI);
                    kh.SDT = _encryptionService.DecryptSDT(kh.SDT);
                }
                catch
                {
                    // Nếu decrypt lỗi (dữ liệu chưa được mã hóa), giữ nguyên
                }
            }
            
            return View(dsKhachHang);
        }

        // GET: Thêm mới
        [HttpGet]
        [PermissionAuthorize(Permissions.KhachHang_Create)]
        public IActionResult Create()
        {
            ViewBag.LoaiKhachHang = _context.LoaiKhachHang.ToList();
            return View();
        }

        // POST: Thêm mới (sinh mã tự động + mã hóa dữ liệu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(Permissions.KhachHang_Create)]
        public async Task<IActionResult> Create(KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                // Sinh mã khách hàng tự động
                khachHang.MAKH = GenerateMaKhachHang();

                // Mã hóa dữ liệu nhạy cảm
                // TENKH: AES-256
                // DIACHI: RSA-2048
                // SDT: Hybrid (AES + RSA)
                khachHang.TENKH = _encryptionService.EncryptTenKH(khachHang.TENKH);
                khachHang.DIACHI = _encryptionService.EncryptDiaChi(khachHang.DIACHI);
                khachHang.SDT = _encryptionService.EncryptSDT(khachHang.SDT);

                _context.KhachHang.Add(khachHang);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                return RedirectToAction(nameof(IndexKH));
            }
            
            ViewBag.LoaiKhachHang = _context.LoaiKhachHang.ToList();
            return View(khachHang);
        }

        // GET: Sửa
        [HttpGet]
        [PermissionAuthorize(Permissions.KhachHang_Edit)]
        public IActionResult Edit(string id)
        {
            if (id == null) return NotFound();

            var kh = _context.KhachHang.FirstOrDefault(x => x.MAKH == id);
            if (kh == null) return NotFound();

            // Decrypt dữ liệu để hiển thị trong form
            try
            {
                kh.TENKH = _encryptionService.DecryptTenKH(kh.TENKH);
                kh.DIACHI = _encryptionService.DecryptDiaChi(kh.DIACHI);
                kh.SDT = _encryptionService.DecryptSDT(kh.SDT);
            }
            catch
            {
                // Giữ nguyên nếu chưa được mã hóa
            }

            ViewBag.LoaiKhachHang = _context.LoaiKhachHang.ToList();
            return View(kh);
        }

        // POST: Sửa (mã hóa lại dữ liệu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(Permissions.KhachHang_Edit)]
        public async Task<IActionResult> Edit(KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Mã hóa lại dữ liệu đã chỉnh sửa
                    khachHang.TENKH = _encryptionService.EncryptTenKH(khachHang.TENKH);
                    khachHang.DIACHI = _encryptionService.EncryptDiaChi(khachHang.DIACHI);
                    khachHang.SDT = _encryptionService.EncryptSDT(khachHang.SDT);

                    _context.Update(khachHang);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
                    return RedirectToAction(nameof(IndexKH));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.KhachHang.Any(e => e.MAKH == khachHang.MAKH))
                        return NotFound();
                    else
                        throw;
                }
            }
            
            ViewBag.LoaiKhachHang = _context.LoaiKhachHang.ToList();
            return View(khachHang);
        }

        // GET: Xóa
        [HttpGet]
        [PermissionAuthorize(Permissions.KhachHang_Delete)]
        public IActionResult Delete(string id)
        {
            if (id == null) return NotFound();

            var kh = _context.KhachHang.FirstOrDefault(x => x.MAKH == id);
            if (kh == null) return NotFound();

            // Decrypt để hiển thị
            try
            {
                kh.TENKH = _encryptionService.DecryptTenKH(kh.TENKH);
                kh.DIACHI = _encryptionService.DecryptDiaChi(kh.DIACHI);
                kh.SDT = _encryptionService.DecryptSDT(kh.SDT);
            }
            catch
            {
                // Giữ nguyên nếu chưa được mã hóa
            }

            return View(kh);
        }

        // POST: Xóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(Permissions.KhachHang_Delete)]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var kh = await _context.KhachHang.FindAsync(id);
            if (kh != null)
            {
                _context.KhachHang.Remove(kh);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa khách hàng thành công!";
            }
            return RedirectToAction(nameof(IndexKH));
        }

        // --------------------------
        // Hàm sinh mã khách hàng tự động
        private string GenerateMaKhachHang()
        {
            var lastKh = _context.KhachHang
                .OrderByDescending(k => k.MAKH)
                .FirstOrDefault();

            if (lastKh == null)
                return "KH001";

            int num = int.Parse(lastKh.MAKH.Substring(2)) + 1;
            return "KH" + num.ToString("D3"); // KH + 3 chữ số, ví dụ KH002
        }
    }
}
