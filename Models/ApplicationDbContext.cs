using Microsoft.EntityFrameworkCore;

namespace QLDH.Models
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các bảng (DbSet)
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<LoaiKhachHang> LoaiKhachHang { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<LoaiSanPham> LoaiSP { get; set; }
        public DbSet<NhaCungCap> NhaCungCap { get; set; }
        public DbSet<SanPham> SanPham { get; set; }
        public DbSet<DonHang> DonHang { get; set; }
        public DbSet<ChiTietDonHang> ChiTietDonHang { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<TaiKhoan> TaiKhoan { get; set; }
        
        // Bảng mới cho Phase 1
        public DbSet<CryptoKey> CryptoKeys { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // Cấu hình bổ sung
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Khóa chính kép cho ChiTietDonHang
            modelBuilder.Entity<ChiTietDonHang>()
                .HasKey(c => new { c.MADH, c.MASP });
            modelBuilder.Entity<KhachHang>().ToTable("KHACHHANG");
            modelBuilder.Entity<TaiKhoan>().ToTable("TAIKHOAN");
            modelBuilder.Entity<NhanVien>().ToTable("NHANVIEN");
            
            // Cấu hình bảng mới
            modelBuilder.Entity<CryptoKey>().ToTable("CRYPTO_KEYS");
            modelBuilder.Entity<AuditLog>().ToTable("AUDIT_LOG");
        }
    }
}
