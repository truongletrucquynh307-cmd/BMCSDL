using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDH.Models
{
    /// <summary>
    /// Bảng ghi lại lịch sử thay đổi dữ liệu
    /// </summary>
    [Table("AUDIT_LOG")]
    public class AuditLog
    {
        [Key]
        [Column("LOG_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("ACTION_TIME")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Column("MANV")]
        [MaxLength(10)]
        public string? MaNV { get; set; }

        [Column("TABLE_NAME")]
        [MaxLength(50)]
        public string EntityType { get; set; } = ""; // KhachHang, DonHang, NhanVien, etc.

        [Column("ACTION_TYPE")]
        [MaxLength(10)]
        public string Action { get; set; } = ""; // INSERT, UPDATE, DELETE

        [Column("RECORD_ID")]
        [MaxLength(50)]
        public string? EntityId { get; set; }

        [Column("OLD_VALUE")]
        public string? OldValues { get; set; } // JSON format

        [Column("NEW_VALUE")]
        public string? NewValues { get; set; } // JSON format

        [Column("IP_ADDRESS")]
        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [Column("DESCRIPTION")]
        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation property
        [ForeignKey("MaNV")]
        public virtual NhanVien? NhanVien { get; set; }
        
        // Computed property để hiển thị tên nhân viên
        [NotMapped]
        public string? UserName => NhanVien?.TENNV ?? MaNV;
    }
}
