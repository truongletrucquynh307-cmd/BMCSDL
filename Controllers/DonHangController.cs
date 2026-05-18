using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDH.Authorization;
using QLDH.Models;
using QLDH.Services.Encryption;
using System.Linq;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class DonHangController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthorizationService _authService;
        private readonly CustomerEncryptionService _encryptionService;

        public DonHangController(
            ApplicationDbContext context, 
            AuthorizationService authService,
            CustomerEncryptionService encryptionService)
        {
            _context = context;
            _authService = authService;
            _encryptionService = encryptionService;
        }

        // ================= INDEX =================
        [PermissionAuthorize(Permissions.DonHang_View)]
        public IActionResult IndexDH()
        {
            var vaiTro = _authService.GetCurrentVaiTro();
            var maNV = _authService.GetCurrentMaNV();

            IQueryable<DonHang> query = _context.DonHang
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien);

            // Quản lý xem được tất cả, Bán hàng/Kho hàng chỉ xem đơn của mình
            if (vaiTro != Roles.QuanLy && !string.IsNullOrEmpty(maNV))
            {
                query = query.Where(d => d.MANV == maNV);
            }

            var list = query
            .OrderBy(d => d.MADH)
            .ToList();

            // Decrypt tên khách hàng để hiển thị
            foreach (var dh in list)
            {
                if (dh.KhachHang != null)
                {
                    try
                    {
                        dh.KhachHang.TENKH = _encryptionService.DecryptTenKH(dh.KhachHang.TENKH);
                    }
                    catch { }
                }
            }

            ViewBag.CurrentVaiTro = vaiTro;
            ViewBag.CurrentMaNV = maNV;
            ViewBag.CanEdit = _authService.CanEditDonHang(maNV);
            
            return View(list);
        }

        // ================= CREATE =================
        [HttpGet]
        [PermissionAuthorize(Permissions.DonHang_Create)]
        public IActionResult Create()
        {
            LoadDropdowns();
            var model = new DonHang
            {
                MADH = GenerateMaDH(),
                NGAYLAP = DateTime.Today,
                MANV = _authService.GetCurrentMaNV() // Gán nhân viên hiện tại
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(Permissions.DonHang_Create)]
        public IActionResult Create(DonHang dh)
        {
            // Kiểm tra dropdown bắt buộc
            if (string.IsNullOrEmpty(dh.MAKH))
                ModelState.AddModelError("MAKH", "Vui lòng chọn khách hàng");

            // Tự động gán nhân viên tạo đơn (nếu không phải Quản lý)
            var vaiTro = _authService.GetCurrentVaiTro();
            var currentMaNV = _authService.GetCurrentMaNV();
            
            if (vaiTro != Roles.QuanLy)
            {
                dh.MANV = currentMaNV; // Bắt buộc là nhân viên hiện tại
            }
            else if (string.IsNullOrEmpty(dh.MANV))
            {
                ModelState.AddModelError("MANV", "Vui lòng chọn nhân viên");
            }

            // Kiểm tra khách hàng / nhân viên tồn tại
            var kh = _context.KhachHang.FirstOrDefault(k => k.MAKH == dh.MAKH);
            var nv = _context.NhanVien.FirstOrDefault(n => n.MANV == dh.MANV);

            if (kh == null) ModelState.AddModelError("MAKH", "Khách hàng không tồn tại");
            if (nv == null && !string.IsNullOrEmpty(dh.MANV)) ModelState.AddModelError("MANV", "Nhân viên không tồn tại");

            if (!ModelState.IsValid)
            {
                LoadDropdowns();
                return View(dh);
            }

            dh.TRANGTHAI = "Đang xử lý";
            dh.NGAYHT = null;

            try
            {
                // Sinh MADH nếu null
                if (string.IsNullOrEmpty(dh.MADH))
                    dh.MADH = GenerateMaDH();

                _context.DonHang.Add(dh);
                _context.SaveChanges();
                
                TempData["SuccessMessage"] = "Tạo đơn hàng thành công!";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                LoadDropdowns();
                return View(dh);
            }

            return RedirectToAction("IndexDH");
        }

        // ================= EDIT =================
        [HttpGet]
        [PermissionAuthorize(Permissions.DonHang_Edit, Permissions.DonHang_EditOwn)]
        public IActionResult Edit(string id)
        {
            if (id == null) return BadRequest();

            var dh = _context.DonHang
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .FirstOrDefault(d => d.MADH == id);
            if (dh == null) return NotFound();

            // Kiểm tra quyền sửa đơn hàng này
            var vaiTro = _authService.GetCurrentVaiTro();
            var currentMaNV = _authService.GetCurrentMaNV();
            
            // Nếu không phải Quản lý, chỉ được sửa đơn của mình (hoặc Kho hàng xem để cập nhật trạng thái)
            if (vaiTro != Roles.QuanLy && vaiTro != Roles.KhoHang && dh.MANV != currentMaNV)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewBag.KhachHangList = GetDecryptedKhachHangList();
            ViewBag.NhanVienList = _context.NhanVien.ToList();
            ViewBag.CurrentVaiTro = vaiTro;
            ViewBag.AllowedStatuses = _authService.GetAllowedDonHangStatuses().ToList();
            
            // Thêm tên khách hàng và nhân viên cho hiển thị (khi không có quyền edit)
            if (dh.KhachHang != null)
            {
                try { ViewBag.TenKH = _encryptionService.DecryptTenKH(dh.KhachHang.TENKH); }
                catch { ViewBag.TenKH = dh.KhachHang.TENKH; }
            }
            if (dh.NhanVien != null)
            {
                ViewBag.TenNV = dh.NhanVien.TENNV;
            }

            return View(dh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(Permissions.DonHang_Edit, Permissions.DonHang_EditOwn)]
        public IActionResult Edit(DonHang model)
        {
            var dh = _context.DonHang
                .Include(d => d.KhachHang)
                .Include(d => d.NhanVien)
                .FirstOrDefault(d => d.MADH == model.MADH);
            if (dh == null) return NotFound();

            // Kiểm tra quyền
            var vaiTro = _authService.GetCurrentVaiTro();
            var currentMaNV = _authService.GetCurrentMaNV();
            
            // Kho hàng được sửa trạng thái bất kỳ đơn nào
            if (vaiTro != Roles.QuanLy && vaiTro != Roles.KhoHang && dh.MANV != currentMaNV)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Kiểm tra quyền thay đổi trạng thái
            var allowedStatuses = _authService.GetAllowedDonHangStatuses();
            if (!string.IsNullOrEmpty(model.TRANGTHAI) && !allowedStatuses.Contains(model.TRANGTHAI))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chuyển sang trạng thái này";
                ViewBag.KhachHangList = GetDecryptedKhachHangList();
                ViewBag.NhanVienList = _context.NhanVien.ToList();
                ViewBag.CurrentVaiTro = vaiTro;
                ViewBag.AllowedStatuses = allowedStatuses.ToList();
                return View(model);
            }

            // Cập nhật trạng thái (Quản lý và Kho hàng)
            if (vaiTro == Roles.QuanLy || vaiTro == Roles.KhoHang)
            {
                // Nếu chuyển sang "Đã giao", tự động cập nhật ngày hoàn thành
                if (model.TRANGTHAI == "Đã giao" && dh.TRANGTHAI != "Đã giao")
                {
                    dh.NGAYHT = DateTime.Today;
                }
                dh.TRANGTHAI = model.TRANGTHAI ?? dh.TRANGTHAI;
            }
            
            // Quản lý và Bán hàng được đổi ngày lập, khách hàng
            if (vaiTro == Roles.QuanLy || vaiTro == Roles.BanHang)
            {
                dh.NGAYLAP = model.NGAYLAP;
                dh.MAKH = model.MAKH;
                
                // Bán hàng cũng được đổi trạng thái giới hạn
                if (vaiTro == Roles.BanHang && !string.IsNullOrEmpty(model.TRANGTHAI) 
                    && allowedStatuses.Contains(model.TRANGTHAI))
                {
                    dh.TRANGTHAI = model.TRANGTHAI;
                }
            }
            
            // Chỉ Quản lý mới được đổi nhân viên phụ trách
            if (vaiTro == Roles.QuanLy)
            {
                dh.MANV = model.MANV;
            }

            try
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật đơn hàng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi cập nhật: " + ex.Message;
            }

            return RedirectToAction("IndexDH");
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(Permissions.DonHang_Delete)]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var dh = _context.DonHang.Find(id);
            if (dh == null) return NotFound();

            // Chỉ Quản lý mới được xóa, hoặc chủ đơn được xóa đơn "Đang xử lý"
            var vaiTro = _authService.GetCurrentVaiTro();
            var currentMaNV = _authService.GetCurrentMaNV();
            
            if (vaiTro != Roles.QuanLy)
            {
                if (dh.MANV != currentMaNV || dh.TRANGTHAI != "Đang xử lý")
                {
                    return RedirectToAction("AccessDenied", "Home");
                }
            }

            _context.DonHang.Remove(dh);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Xóa đơn hàng thành công!";
            
            return RedirectToAction(nameof(IndexDH));
        }

        // Sinh MADH tự động
        private string GenerateMaDH()
        {
            var last = _context.DonHang.OrderByDescending(d => d.MADH).FirstOrDefault();
            if (last == null) return "DH001";
            var number = int.Parse(last.MADH.Substring(2)) + 1;
            return "DH" + number.ToString("D3");
        }

        private void LoadDropdowns()
        {
            var khachHangList = GetDecryptedKhachHangList();
            ViewBag.KhachHangList = new SelectList(khachHangList, "MAKH", "TENKH");
            ViewBag.NhanVienList = new SelectList(_context.NhanVien.ToList(), "MANV", "TENNV");
            ViewBag.CurrentVaiTro = _authService.GetCurrentVaiTro();
        }
        
        // Lấy danh sách khách hàng với tên đã giải mã
        private List<KhachHang> GetDecryptedKhachHangList()
        {
            var list = _context.KhachHang.ToList();
            foreach (var kh in list)
            {
                try
                {
                    kh.TENKH = _encryptionService.DecryptTenKH(kh.TENKH);
                }
                catch { }
            }
            return list;
        }
    }
}
