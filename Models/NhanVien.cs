using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDH.Models
{
    [Table("NHANVIEN")] 
    public class NhanVien
    {
        [Key]
        [Column("MANV")]
        [StringLength(5)]
        public string MANV { get; set; }

        [Column("TENNV")]
        [StringLength(100)]
        [Required]
        public string TENNV { get; set; }

        [Column("CHUCVU")]
        [StringLength(50)]
        public string CHUCVU { get; set; }

        [Column("DIACHI")]
        [StringLength(200)]
        public string DIACHI { get; set; }

        [Column("SDT")]
        [StringLength(15)]
        public string SDT { get; set; }

        [Column("EMAIL")]
        [StringLength(100)]
        public string EMAIL { get; set; }
    }
}
