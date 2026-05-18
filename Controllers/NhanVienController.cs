using Microsoft.AspNetCore.Mvc;
using QLDH.Authorization;
using QLDH.Models;
using QLDH.Services;
using System.Linq;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthorizationService _authService;
        private readonly OracleSecurityService _securityService;
        private readonly NhanVienDecryptionService _decryptionService;

        public NhanVienController(
            ApplicationDbContext context, 
            AuthorizationService authService,
            OracleSecurityService securityService,
            NhanVienDecryptionService decryptionService)
        {
            _context = context;
            _authService = authService;
            _securityService = securityService;
            _decryptionService = decryptionService;
        }

        // ================= INDEX =================
        [PermissionAuthorize(Permissions.NhanVien_View)]
        public IActionResult IndexNV()
        {
            var vaiTro = _authService.GetCurrentVaiTro();
            var currentMaNV = _authService.GetCurrentMaNV();

            List<NhanVien> list;
            
            // Quản lý xem được tất cả, nhân viên khác chỉ xem thông tin của mình
            if (vaiTro == Roles.QuanLy)
            {
                list = _context.NhanVien
                    .OrderBy(n => n.MANV)
                    .ToList();
            }
            else
            {
                // Nhân viên chỉ xem được thông tin của bản thân
                list = _context.NhanVien
                    .Where(n => n.MANV == currentMaNV)
                    .ToList();
            }

            // ✅ Giải mã DIACHI và SDT nếu đã được mã hóa ở DB level
            foreach (var nv in list)
            {
                // Dùng C# service để giải mã (đồng bộ key với Oracle)
                nv.DIACHI = _decryptionService.DecryptDiaChi(nv.DIACHI);
                nv.SDT = _decryptionService.DecryptSDT(nv.SDT);
            }

            ViewBag.CurrentVaiTro = vaiTro;
            ViewBag.CurrentMaNV = currentMaNV;
            ViewBag.CanCreate = _authService.HasPermission(Permissions.NhanVien_Create);
            ViewBag.CanDelete = _authService.HasPermission(Permissions.NhanVien_Delete);
            
            return View(list);
        }

        // ================= CREATE =================
        [HttpGet]
        [PermissionAuthorize(Permissions.NhanVien_Create)]
        public IActionResult Create()
        {
            var model = new NhanVien
            {
                MANV = GenerateMaNV()
            };

            ViewBag.ChucVuList = new[] { "Quản lý", "Bán hàng", "Kho hàng" };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(Permissions.NhanVien_Create)]
        public IActionResult Create(NhanVien nv)
        {
            if (ModelState.IsValid)
            {
                // Đảm bảo mã NV được sinh tự động
                if (string.IsNullOrEmpty(nv.MANV))
                    nv.MANV = GenerateMaNV();

                _context.Set<NhanVien>().Add(nv);
                _context.SaveChanges();
                
                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(IndexNV));
            }
            
            ViewBag.ChucVuList = new[] { "Quản lý", "Bán hàng", "Kho hàng" };
            return View(nv);
        }

        // ================= EDIT =================
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var nv = _context.Set<NhanVien>().Find(id);
            if (nv == null) return NotFound();

            // Kiểm tra quyền sửa
            var vaiTro = _authService.GetCurrentVaiTro();
            var currentMaNV = _authService.GetCurrentMaNV();
            
            // Kiểm tra quyền: Quản lý có NhanVien_Edit, nhân viên khác có NhanVien_EditSelf (chỉ sửa bản thân)
            bool canEdit = _authService.HasPermission(Permissions.NhanVien_Edit) ||
                          (_authService.HasPermission(Permissions.NhanVien_EditSelf) && currentMaNV == id);
            
            if (!canEdit)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa thông tin nhân viên này!";
                return RedirectToAction("Dashboard", "Account");
            }

            // ✅ Giải mã DIACHI và SDT trước khi hiển thị form
            nv.DIACHI = _decryptionService.DecryptDiaChi(nv.DIACHI);
            nv.SDT = _decryptionService.DecryptSDT(nv.SDT);

            ViewBag.CurrentVaiTro = vaiTro;
            ViewBag.IsEditingSelf = (currentMaNV == id);
            ViewBag.ChucVuList = new[] { "Quản lý", "Bán hàng", "Kho hàng" };
            
            return View(nv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, NhanVien nv)
        {
            if (id != nv.MANV) return NotFound();

            var currentMaNV = _authService.GetCurrentMaNV();
            
            // Kiểm tra quyền: Quản lý có NhanVien_Edit, nhân viên khác có NhanVien_EditSelf (chỉ sửa bản thân)
            bool canEdit = _authService.HasPermission(Permissions.NhanVien_Edit) ||
                          (_authService.HasPermission(Permissions.NhanVien_EditSelf) && currentMaNV == id);
            
            if (!canEdit)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền sửa thông tin nhân viên này!";
                return RedirectToAction("Dashboard", "Account");
            }

            var existingNV = _context.Set<NhanVien>().Find(id);
            if (existingNV == null) return NotFound();

            var vaiTro = _authService.GetCurrentVaiTro();
            var isEditingSelf = (currentMaNV == id);

            if (ModelState.IsValid)
            {
                // Cập nhật các trường cho phép
                existingNV.TENNV = nv.TENNV;
                existingNV.SDT = nv.SDT;
                existingNV.EMAIL = nv.EMAIL;
                existingNV.DIACHI = nv.DIACHI;
                
                // Chỉ Quản lý mới được đổi Chức vụ (và không được tự đổi chức vụ của mình)
                if (vaiTro == Roles.QuanLy && !isEditingSelf)
                {
                    existingNV.CHUCVU = nv.CHUCVU;
                }
                // Nếu tự sửa bản thân, không cho đổi chức vụ
                
                _context.SaveChanges();
                
                TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                return RedirectToAction(nameof(IndexNV));
            }
            
            ViewBag.CurrentVaiTro = vaiTro;
            ViewBag.IsEditingSelf = isEditingSelf;
            ViewBag.ChucVuList = new[] { "Quản lý", "Bán hàng", "Kho hàng" };
            
            return View(nv);
        }

        // ================= DELETE (chỉ Quản lý) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(Permissions.NhanVien_Delete)]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var currentMaNV = _authService.GetCurrentMaNV();
            
            // Không cho xóa chính mình
            if (id == currentMaNV)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản của chính bạn!";
                return RedirectToAction(nameof(IndexNV));
            }

            var nv = _context.Set<NhanVien>().Find(id);
            if (nv != null)
            {
                // Kiểm tra xem nhân viên có đơn hàng nào không
                var hasDonHang = _context.DonHang.Count(d => d.MANV == id) > 0;
                if (hasDonHang)
                {
                    TempData["ErrorMessage"] = "Không thể xóa nhân viên này vì đã có đơn hàng";
                    return RedirectToAction(nameof(IndexNV));
                }

                _context.Set<NhanVien>().Remove(nv);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            return RedirectToAction(nameof(IndexNV));
        }

        // ================= Sinh mã tự động =================
        private string GenerateMaNV()
        {
            // Lấy nhân viên có mã cao nhất
            var last = _context.NhanVien
                .OrderByDescending(n => n.MANV)
                .FirstOrDefault();

            if (last == null) return "NV001"; // nếu chưa có NV nào

            // Lấy số cuối mã, ví dụ NV012 -> 12
            int num = 0;
            if (int.TryParse(last.MANV.Substring(2), out num))
            {
                num += 1;
            }
            else
            {
                num = 1;
            }

            return "NV" + num.ToString("D3"); // NV001, NV002,...
        }
    }
}
