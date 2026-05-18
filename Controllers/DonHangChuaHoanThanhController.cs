using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDH.Models;
using QLDH.Authorization;

namespace QLDH.Controllers
{
    [RequireLogin]
    public class DonHangChuaHoanThanhController : Controller
{
    private readonly ApplicationDbContext _context;

    public DonHangChuaHoanThanhController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: DonHangChuaHoanThanh
    public async Task<IActionResult> Index()
    {
        // Lấy tất cả đơn hàng (chỉ để debug)
        var allDonHang = await _context.DonHang
            .Include(d => d.KhachHang)
            .Include(d => d.NhanVien)
            .ToListAsync();

        // In debug để kiểm tra giá trị TRANGTHAI
        foreach (var dh in allDonHang)
        {
            Console.WriteLine($"MADH={dh.MADH}, TRANGTHAI='{dh.TRANGTHAI}'");
        }

        // Lọc đơn hàng đang xử lý (loại bỏ khoảng trắng và chữ hoa/chữ thường)
        var donHangDangXuLy = allDonHang
            .Where(d => !string.IsNullOrEmpty(d.TRANGTHAI) && d.TRANGTHAI.Trim().ToLower() == "đang xử lý")
            .ToList();

        // Trả về view với model là danh sách đã lọc
        return View(donHangDangXuLy);
    }

    // GET: Chỉnh trạng thái đơn hàng
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
            return NotFound();

        var dh = await _context.DonHang.FindAsync(id);
        if (dh == null)
            return NotFound();

        // Dropdown trạng thái
        ViewBag.TrangThaiList = new List<string> { "Đang xử lý", "Đã giao", "Đã hủy" };

        return View(dh);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DonHang dh)
    {
        if (dh == null || string.IsNullOrEmpty(dh.MADH))
            return BadRequest();

        // Lấy đơn hàng hiện tại từ database
        var donHang = await _context.DonHang.FindAsync(dh.MADH);
        if (donHang == null)
            return NotFound();

        // Chỉ cập nhật trạng thái
        donHang.TRANGTHAI = dh.TRANGTHAI;

        await _context.SaveChangesAsync();

        // Quay về danh sách đơn hàng chưa hoàn thành
        return RedirectToAction(nameof(Index));
    }


    private bool DonHangExists(string id)
    {
        return _context.DonHang.Any(e => e.MADH == id);
    }
}
}
