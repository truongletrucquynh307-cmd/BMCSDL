using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDH.Authorization;
using QLDH.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class HoaDonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HoaDonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public IActionResult IndexHD()
        {
            var list = _context.HoaDon
                .Include(h => h.DonHang)
                .OrderBy(h => h.MAHD)
                .ToList();
            return View(list);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create()
        {
            // Lấy danh sách Đơn Hàng cho dropdown
            var donHangList = _context.DonHang
                .OrderBy(d => d.MADH)
                .Select(d => new SelectListItem
                {
                    Value = d.MADH,
                    Text = d.MADH
                }).ToList();

            ViewBag.DonHangList = donHangList;

            // Phương thức thanh toán
            ViewBag.PhuongThucTTList = new List<SelectListItem>
            {
             new SelectListItem { Value="Tiền mặt", Text="Tiền mặt" },
             new SelectListItem { Value="Chuyển khoản", Text="Chuyển khoản" }
            };

            // Khởi tạo model
            var model = new HoaDon
            {
                MAHD = GenerateMaHD(),
                NGAYLAP = DateTime.Today,
                TONGTIEN = 0
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult GetTongTien(string madh)
        {
            if (string.IsNullOrEmpty(madh))
                return Json(0);

            var tongTien = _context.ChiTietDonHang
                .Where(c => c.MADH == madh)
                .Sum(c => (decimal?)c.DONGIA * c.SOLUONG) ?? 0;

            return Json(tongTien);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HoaDon model)
        {
            if (string.IsNullOrEmpty(model.MADH))
            {
                ModelState.AddModelError("MADH", "Vui lòng chọn mã đơn hàng");
            }

            // Tính TONGTIEN tự động
            var chiTietList = await _context.ChiTietDonHang
                .Where(c => c.MADH == model.MADH)
                .ToListAsync();
            model.TONGTIEN = chiTietList.Sum(c => c.DONGIA * c.SOLUONG);

            if (!ModelState.IsValid)
            {
                // Điền lại dropdown nếu form lỗi
                var donHangList = _context.DonHang.OrderBy(d => d.MADH).ToList();

                ViewBag.DonHangList = donHangList
                    .Select(d => new SelectListItem
                    {
                        Value = d.MADH,
                        Text = d.MADH
                    }).ToList();

                ViewBag.TongTienDict = donHangList.ToDictionary(
                    d => d.MADH,
                    d => _context.ChiTietDonHang
                            .Where(c => c.MADH == d.MADH)
                            .Sum(c => (decimal?)c.DONGIA * c.SOLUONG) ?? 0
                );

                ViewBag.PhuongThucTTList = new List<SelectListItem>
                {
                    new SelectListItem { Value="Tiền mặt", Text="Tiền mặt" },
                    new SelectListItem { Value="Chuyển khoản", Text="Chuyển khoản" }
                };

                return View(model);
            }

            _context.HoaDon.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("IndexHD");
        }

        // ================= EDIT =================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
                return NotFound();

            var hoaDon = await _context.HoaDon.FindAsync(id);
            if (hoaDon == null)
                return NotFound();

            // Load dropdown phương thức thanh toán
            ViewBag.PhuongThucTTList = new List<SelectListItem>
    {
        new SelectListItem { Value = "Tiền mặt", Text = "Tiền mặt", Selected = hoaDon.PHUONGTHUCTT == "Tiền mặt" },
        new SelectListItem { Value = "Chuyển khoản", Text = "Chuyển khoản", Selected = hoaDon.PHUONGTHUCTT == "Chuyển khoản" }
    };

            // Không cần tính lại TONGTIEN, hiển thị giá trị đã lưu
            return View(hoaDon);
        }

        // ================= DELETE =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HoaDon model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.HoaDon.FindAsync(model.MAHD);
                if (existing == null) return NotFound();

                existing.NGAYLAP = model.NGAYLAP;
                existing.PHUONGTHUCTT = model.PHUONGTHUCTT;
                existing.TONGTIEN = model.TONGTIEN;

                _context.Update(existing);
                await _context.SaveChangesAsync();

                return RedirectToAction("IndexHD");
            }

            // Nếu form lỗi, load dropdown
            ViewBag.PhuongThucTTList = new List<SelectListItem>
    {
        new SelectListItem { Value = "Tiền mặt", Text = "Tiền mặt", Selected = model.PHUONGTHUCTT == "Tiền mặt" },
        new SelectListItem { Value = "Chuyển khoản", Text = "Chuyển khoản", Selected = model.PHUONGTHUCTT == "Chuyển khoản" }
    };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var hoaDon = await _context.HoaDon.FindAsync(id);
            if (hoaDon == null)
                return NotFound();

            _context.HoaDon.Remove(hoaDon);
            await _context.SaveChangesAsync();

            return RedirectToAction("IndexHD");
        }


        // Sinh MAHD tự động
        private string GenerateMaHD()
        {
            var last = _context.HoaDon.OrderByDescending(h => h.MAHD).FirstOrDefault();
            if (last == null) return "HD001";

            var number = int.Parse(last.MAHD.Substring(2)) + 1;
            return "HD" + number.ToString("D3");
        }
    }
}
