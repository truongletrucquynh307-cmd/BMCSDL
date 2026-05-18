using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDH.Models
{
    [Table("SANPHAM")]
    public class SanPham
    {
        [Key]
        [StringLength(5)]
        public string MASP { get; set; }

        [Required]
        [StringLength(100)]
        public string TENSP { get; set; }

        public decimal GIANHAP { get; set; }
        public decimal GIABAN { get; set; }
        public int SOLUONGTON { get; set; }

        [StringLength(200)]
        public string GHICHU { get; set; }

        public DateTime NGAYTAO { get; set; }
    }
}
