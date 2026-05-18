using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDH.Models
{
    [Table("DONHANG")]
    public class DonHang
    {
        [Key]
        [StringLength(5)]
        [Column("MADH")]
        public string MADH { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Column("NGAYLAP")]
        public DateTime NGAYLAP { get; set; }

        [DataType(DataType.Date)]
        [Column("NGAYHT")]
        public DateTime? NGAYHT { get; set; }

        [StringLength(50)]
        [Column("TRANGTHAI")]
        public string TRANGTHAI { get; set; } = "Đang xử lý";

        [Required(ErrorMessage = "Chọn khách hàng")]
        public string MAKH { get; set; }

        [Required(ErrorMessage = "Chọn nhân viên")]
        public string MANV { get; set; }

        [ForeignKey("MAKH")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MANV")]
        public virtual NhanVien? NhanVien { get; set; }

    }
}
