using Microsoft.EntityFrameworkCore;
using QLDH.Models; // nơi chứa LoaiKhachHang

namespace QLDH.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet tương ứng với các bảng
        public DbSet<LoaiKhachHang> LoaiKhachHang { get; set; }

        // Thêm DbSet cho bảng khác nếu có, ví dụ:
        // public DbSet<KhachHang> KhachHangs { get; set; }
    }
}
