using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using QLDH.Authorization;
using QLDH.Models;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class ChiTietDonHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChiTietDonHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===================== INDEX =====================
        public async Task<IActionResult> IndexCTDH()
        {
            var data = await _context.ChiTietDonHang
                .Include(c => c.DonHang)
                .Include(c => c.SanPham)
                .OrderBy(c => c.MADH)
                .ToListAsync();

            return View(data);
        }

        // ===================== CREATE =====================
        [HttpGet]
        public IActionResult Create()
        {
            var donhangList = _context.DonHang.ToList();
            var sanphamList = _context.SanPham.ToList();

            if (donhangList == null || sanphamList == null)
            {
                return Content("Lỗi: Không load được dữ liệu DonHang hoặc SanPham. Kiểm tra tên bảng Oracle.");
            }

            ViewBag.DonHangList = donhangList;
            ViewBag.SanPhamList = sanphamList;

            return View(new ChiTietDonHang());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ChiTietDonHang model)
        {
            if (ModelState.IsValid)
            {
                // Lấy giá bán từ database dựa trên MASP
                var sp = await _context.SanPham.FirstOrDefaultAsync(s => s.MASP == model.MASP);
                if (sp != null)
                {
                    model.DONGIA = sp.GIABAN; // gán đơn giá tự động
                }

                _context.ChiTietDonHang.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("IndexCTDH");
            }

            ViewBag.DonHangList = _context.DonHang.ToList();
            ViewBag.SanPhamList = _context.SanPham.ToList();
            return View(model);
        }

        // ===================== EDIT =====================
        [HttpGet]
        public async Task<IActionResult> Edit(string MADH, string MASP)
        {
            if (string.IsNullOrEmpty(MADH) || string.IsNullOrEmpty(MASP))
                return NotFound();

            var ctdh = await _context.ChiTietDonHang
                .FirstOrDefaultAsync(c => c.MADH == MADH && c.MASP == MASP);

            if (ctdh == null)
                return NotFound();

            // Dropdown Mã Đơn hàng
            ViewBag.DonHangList = _context.DonHang
                .Select(d => new SelectListItem
                {
                    Value = d.MADH,
                    Text = d.MADH
                }).ToList();

            // Danh sách sản phẩm thô để thêm data-price
            ViewBag.SanPhamListRaw = _context.SanPham.ToList();

            return View(ctdh);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ChiTietDonHang model)
        {
            if (ModelState.IsValid)
            {
                // Lấy giá bán tự động từ SP
                var sp = await _context.SanPham.FirstOrDefaultAsync(s => s.MASP == model.MASP);
                if (sp != null)
                    model.DONGIA = sp.GIABAN;

                try
                {
                    _context.ChiTietDonHang.Update(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("IndexCTDH");
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Dữ liệu đã bị thay đổi, vui lòng thử lại.");
                }
            }

            // Nếu ModelState invalid, fill lại ViewBag
            ViewBag.DonHangList = _context.DonHang
                .Select(d => new SelectListItem
                {
                    Value = d.MADH,
                    Text = d.MADH
                }).ToList();

            ViewBag.SanPhamListRaw = _context.SanPham.ToList();

            return View(model);
        }


        // ===================== DELETE =====================
        // GET: ChiTietDonHang/Delete
        [HttpGet]
        public async Task<IActionResult> Delete(string MADH, string MASP)
        {
            if (MADH == null || MASP == null)
                return NotFound();

            var ctdh = await _context.ChiTietDonHang
                .Include(c => c.DonHang)
                .Include(c => c.SanPham)
                .FirstOrDefaultAsync(c => c.MADH == MADH && c.MASP == MASP);

            if (ctdh == null)
                return NotFound();

            return View(ctdh); // Optional, modal sẽ xử lý nên view riêng Delete không bắt buộc
        }

        // POST: ChiTietDonHang/DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string MADH, string MASP)
        {
            if (MADH == null || MASP == null)
                return NotFound();

            var ctdh = await _context.ChiTietDonHang
                .FirstOrDefaultAsync(c => c.MADH == MADH && c.MASP == MASP);

            if (ctdh == null)
                return NotFound();

            _context.ChiTietDonHang.Remove(ctdh);
            await _context.SaveChangesAsync();

            return RedirectToAction("IndexCTDH");
        }
    }
}
