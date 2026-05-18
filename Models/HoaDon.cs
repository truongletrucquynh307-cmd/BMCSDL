using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDH.Models
{
    [Table("HOADON")]
    public class HoaDon
    {
        [Key]
        [StringLength(5)]
        public string MAHD { get; set; }

        [ForeignKey("DonHang")]
        public string? MADH { get; set; }
        public DonHang? DonHang { get; set; }

        public DateTime NGAYLAP { get; set; }

        [StringLength(50)]
        public string PHUONGTHUCTT { get; set; }

        public decimal TONGTIEN { get; set; }
    }
}
