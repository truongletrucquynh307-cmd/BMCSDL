using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QLDH.Models
{
    [Table("CHITIETDONHANG")]  // tên bảng Oracle
    public class ChiTietDonHang
    {
        [Key] // Nếu cần đánh dấu composite key thì dùng Fluent API trong DbContext
        [Column("MADH")]
        [StringLength(5)]
        [Required(ErrorMessage = "Chọn đơn hàng")]
        public string MADH { get; set; }

        [Column("MASP")]
        [StringLength(5)]
        [Required(ErrorMessage = "Chọn sản phẩm")]
        public string MASP { get; set; }

        [Column("SOLUONG")]
        [Required(ErrorMessage = "Nhập số lượng")]
        public int SOLUONG { get; set; }

        [Column("DONGIA")]
        [Required(ErrorMessage = "Nhập đơn giá")]
        public decimal DONGIA { get; set; }

        // Navigation property - không bắt buộc validate khi submit form
        [BindNever]
        [ForeignKey("MADH")]
        public virtual DonHang? DonHang { get; set; }

        [BindNever]
        [ForeignKey("MASP")]
        public virtual SanPham? SanPham { get; set; }
    }
}
