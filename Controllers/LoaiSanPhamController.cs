using Microsoft.AspNetCore.Mvc;
using QLDH.Authorization;
using QLDH.Models;
using System.Linq;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class LoaiSanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoaiSanPhamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public IActionResult IndexLSP()
        {
            var list = _context.LoaiSP
                      .OrderBy(n => n.MALOAISP)   
                      .ToList();
            return View(list);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create()
        {
            var model = new LoaiSanPham
            {
                MALOAISP = GenerateMaLSP() // sinh mã tự động
            };
            return View(model); // View: Create.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LoaiSanPham lsp)
        {
            if (ModelState.IsValid)
            {
                _context.LoaiSP.Add(lsp); // dùng DbSet LoaiSP
                _context.SaveChanges();
                return RedirectToAction(nameof(IndexLSP));
            }
            return View(lsp);
        }

        // ================= EDIT =================
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var lsp = _context.LoaiSP.Find(id);
            if (lsp == null) return NotFound();
            return View(lsp); // View: Edit.cshtml
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, LoaiSanPham lsp)
        {
            if (id != lsp.MALOAISP) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(lsp);
                _context.SaveChanges();
                return RedirectToAction(nameof(IndexLSP));
            }
            return View(lsp);
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest();

            var lsp = _context.LoaiSP.Find(id);
            if (lsp != null)
            {
                _context.LoaiSP.Remove(lsp);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(IndexLSP));
        }

        // ================= Sinh mã tự động =================
        private string GenerateMaLSP()
        {
            var last = _context.LoaiSP
                .OrderByDescending(l => l.MALOAISP)
                .FirstOrDefault();

            if (last == null)
                return "LSP001";

            int num = int.Parse(last.MALOAISP.Substring(3)) + 1;
            return "LSP" + num.ToString("D3");
        }
    }
}
