using Microsoft.EntityFrameworkCore;

namespace QLDH.Models
{
    public class QLDHContext : DbContext
    {
        public QLDHContext(DbContextOptions<QLDHContext> options)
            : base(options)
        {
        }

        public DbSet<SanPham> SanPham { get; set; }
        public object NhanVien { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SanPham>().ToTable("SANPHAM");
        
            base.OnModelCreating(modelBuilder);
        }
    }
}
