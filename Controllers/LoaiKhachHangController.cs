using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDH.Authorization;
using QLDH.Data;
using QLDH.Models;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class LoaiKhachHangController : Controller
    {
        private readonly AppDbContext _context;

        public LoaiKhachHangController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexLoaiKH()
        {
            var list = await _context.LoaiKhachHang
                .AsNoTracking()
                .OrderBy(x => x.MALOAIKH)
                .ToListAsync();
            return View("IndexLoaiKH", list);
        }

        [HttpGet]
        public async Task<IActionResult> CreateLoaiKH()
        {
            var model = new LoaiKhachHang
            {
                MALOAIKH = await GenerateMaLoaiKhachHangAsync()
            };

            return View("CreateLoaiKH", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLoaiKH(LoaiKhachHang model)
        {
            if (!ModelState.IsValid)
                return View("CreateLoaiKH", model);

            var count = await _context.LoaiKhachHang
                .CountAsync(x => x.MALOAIKH == model.MALOAIKH);

            if (count > 0)
            {
                var newCode = await GenerateMaLoaiKhachHangAsync();
                ModelState.AddModelError(nameof(model.MALOAIKH), $"Mã cũ đã bị trùng. Hệ thống đã cập nhật mã mới: {newCode}. Vui lòng nhấn Lưu lại.");
                model.MALOAIKH = newCode;
                return View("CreateLoaiKH", model);
            }

            _context.LoaiKhachHang.Add(model);
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexLoaiKH));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Không thể lưu dữ liệu do lỗi hệ thống. Chi tiết: " + ex.Message);
                model.MALOAIKH = await GenerateMaLoaiKhachHangAsync();
                return View("CreateLoaiKH", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditLoaiKH(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var loaiKH = await _context.LoaiKhachHang
                .FirstOrDefaultAsync(x => x.MALOAIKH == id);
            if (loaiKH == null) return NotFound();

            return View("EditLoaiKH", loaiKH);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLoaiKH(LoaiKhachHang model)
        {
            if (model == null || string.IsNullOrEmpty(model.MALOAIKH)) return NotFound();

            if (!ModelState.IsValid)
                return View("EditLoaiKH", model);

            var existing = await _context.LoaiKhachHang
                .FirstOrDefaultAsync(x => x.MALOAIKH == model.MALOAIKH);
            if (existing == null) return NotFound();

            existing.TENLOAIKH = model.TENLOAIKH;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexLoaiKH));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật dữ liệu.");
                return View("EditLoaiKH", model);
            }
        }

        [HttpGet]
        public IActionResult DeleteLoaiKH(string id)
        {
            return RedirectToAction(nameof(IndexLoaiKH));
        }

        [HttpPost, ActionName("DeleteLoaiKH")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLoaiKHConfirmed(string MALOAIKH)
        {
            if (string.IsNullOrEmpty(MALOAIKH)) return RedirectToAction(nameof(IndexLoaiKH));

            var loaiKH = await _context.LoaiKhachHang
                .FirstOrDefaultAsync(x => x.MALOAIKH == MALOAIKH);
            if (loaiKH == null) return RedirectToAction(nameof(IndexLoaiKH));

            _context.LoaiKhachHang.Remove(loaiKH);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexLoaiKH));
        }

        private async Task<string> GenerateMaLoaiKhachHangAsync()
        {
            var codes = await _context.LoaiKhachHang
                .Select(l => l.MALOAIKH)
                .ToListAsync();

            string prefix = "LK";
            int prefixLength = prefix.Length;

            var nums = codes
                .Where(c => !string.IsNullOrWhiteSpace(c) && c.Length > prefixLength && c.StartsWith(prefix))
                .Select(c =>
                {
                    var s = c.Substring(prefixLength);
                    return int.TryParse(s, out var n) ? (int?)n : null;
                })
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .OrderBy(n => n)
                .ToList();

            int expected = 1;
            foreach (var n in nums)
            {
                if (n == expected) expected++;
                else if (n > expected) break;
            }

            return prefix + expected.ToString("D3");
        }
    }
}
