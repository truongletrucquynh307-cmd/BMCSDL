using Microsoft.AspNetCore.Mvc;
using QLDH.Authorization;
using QLDH.Models;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class NhaCungCapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhaCungCapController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= INDEX =================
        public IActionResult IndexNCC()
        {
            var list = _context.NhaCungCap
                      .OrderBy(n => n.MANCC)   // Sắp xếp theo mã NV tăng dần
                      .ToList();
            return View(list);
        }

        // ================= CREATE =================
        [HttpGet]
        public IActionResult Create()
        {
            var model = new NhaCungCap
            {
                MANCC = GenerateMaNCC()
            };
            ViewData["ShowFooter"] = false; 
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(NhaCungCap ncc)
        {
            if (ModelState.IsValid)
            {
                _context.NhaCungCap.Add(ncc);
                _context.SaveChanges();
                return RedirectToAction(nameof(IndexNCC));
            }
            return View(ncc);
        }

        // ================= EDIT =================
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (id == null) return NotFound();

            var ncc = _context.NhaCungCap.Find(id);
            if (ncc == null) return NotFound();

            return View(ncc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, NhaCungCap ncc)
        {
            if (id != ncc.MANCC) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(ncc);
                _context.SaveChanges();
                return RedirectToAction(nameof(IndexNCC));
            }
            return View(ncc);
        }

        // ================= DELETE =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string MANCC)
        {
            if (string.IsNullOrEmpty(MANCC))
                return BadRequest();

            var ncc = _context.NhaCungCap.Find(MANCC);
            if (ncc != null)
            {
                _context.NhaCungCap.Remove(ncc);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(IndexNCC));
        }


        // Hàm sinh mã NCC tự động
        private string GenerateMaNCC()
        {
            var lastNcc = _context.NhaCungCap
                .OrderByDescending(n => n.MANCC)
                .FirstOrDefault();

            if (lastNcc == null)
                return "NCC001";

            int num = int.Parse(lastNcc.MANCC.Substring(3)) + 1;
            return "NCC" + num.ToString("D3"); // NCC001, NCC002, ...
        }

    }
}
