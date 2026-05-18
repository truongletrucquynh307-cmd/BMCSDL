using Microsoft.AspNetCore.Mvc;
using QLDH.Authorization;
using QLDH.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class SanPhamController : Controller
    {
        private readonly QLDHContext _context;

        public SanPhamController(QLDHContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexSP()
        {
            var sanPham = await _context.SanPham
                .AsNoTracking()
                .OrderBy(x => x.MASP)
                .ToListAsync();

            return View(sanPham);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSP()
        {
            var model = new SanPham
            {
                MASP = await GenerateMaSanPhamAsync(),
                NGAYTAO = DateTime.Now
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSP(SanPham sp)
        {
            if (ModelState.IsValid)
            {
                var count = await _context.SanPham.CountAsync(s => s.MASP == sp.MASP);

                if (count > 0)
                {
                    ModelState.AddModelError("MASP", $"Mã sản phẩm '{sp.MASP}' đã tồn tại. Hệ thống đã cập nhật mã mới: {await GenerateMaSanPhamAsync()}. Vui lòng thử lưu lại.");
                    sp.MASP = await GenerateMaSanPhamAsync();
                    sp.NGAYTAO = DateTime.Now;
                    return View(sp);
                }

                sp.NGAYTAO = DateTime.Now;
                _context.SanPham.Add(sp);

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction(nameof(IndexSP));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi lưu dữ liệu. Vui lòng kiểm tra lại ràng buộc: " + ex.Message);
                    sp.NGAYTAO = DateTime.Now;
                    return View(sp);
                }
            }

            sp.NGAYTAO = DateTime.Now;
            return View(sp);
        }

        [HttpGet]
        public async Task<IActionResult> EditSP(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var sp = await _context.SanPham.FindAsync(id);
            if (sp == null) return NotFound();

            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSP(SanPham sp)
        {
            ModelState.Remove(nameof(sp.NGAYTAO));

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSp = await _context.SanPham.AsNoTracking().FirstOrDefaultAsync(s => s.MASP == sp.MASP);
                    if (existingSp == null) return NotFound();

                    sp.NGAYTAO = existingSp.NGAYTAO;

                    _context.Update(sp);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(IndexSP));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SanPham.Any(e => e.MASP == sp.MASP))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật dữ liệu: " + ex.Message);
                }
            }
            return View(sp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSP(string id)
        {
            try
            {
                var sp = await _context.SanPham.FirstOrDefaultAsync(s => s.MASP == id);
                if (sp == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy sản phẩm cần xóa!";
                    return RedirectToAction(nameof(IndexSP));
                }

                _context.SanPham.Remove(sp);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
            }

            return RedirectToAction(nameof(IndexSP));
        }

        private async Task<string> GenerateMaSanPhamAsync()
        {
            var codes = await _context.SanPham
                .Select(s => s.MASP)
                .ToListAsync();

            string prefix = "SP";
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
