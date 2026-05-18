using System.Linq;
using QLDH.Models;

namespace QLDH.Services
{
    public class KhachHangService
    {
        private readonly ApplicationDbContext _context;

        public KhachHangService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hàm sinh mã khách hàng tự động
        public string GenerateCustomerId()
        {
            var lastKH = _context.KhachHang
                .OrderByDescending(k => k.MAKH)
                .FirstOrDefault();

            string newID = "KH0001";

            if (lastKH != null)
            {
                string numberPart = lastKH.MAKH.Substring(2); // bỏ "KH"
                int nextNumber = int.Parse(numberPart) + 1;
                newID = "KH" + nextNumber.ToString("D4"); // format 4 số
            }

            return newID;
        }
    }
}
