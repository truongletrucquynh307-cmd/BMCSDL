using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDH.Models
{
    [Table("LOAISP")] // map trực tiếp với bảng LOAISP trong Oracle
    public class LoaiSanPham
    {
        [Key]
        [Column("MALOAISP")]
        public string MALOAISP { get; set; }

        [NotMapped]
        public string MALSP
        {
            get => MALOAISP;
            set => MALOAISP = value;
        }

        [Column("TENLOAI")]
        [Required]
        public string TENLOAI { get; set; }
    }
}
