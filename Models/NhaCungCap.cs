using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDH.Models
{
    [Table("NCC")]
    public class NhaCungCap
    {
        [Key]
        [Column("MANCC")]
        public string MANCC { get; set; } = null!;

        [Column("TENNCC")]
        public string? TENNCC { get; set; }

        [Column("DIACHI")]
        public string? DIACHI { get; set; }

        [Column("SDT")]
        public string? SDT { get; set; }

        [EmailAddress]
        [Column("EMAIL")]
        public string? EMAIL { get; set; }
    }
}
