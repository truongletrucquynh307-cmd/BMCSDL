using System.ComponentModel.DataAnnotations;

namespace QLDH.Models
{
    public class TaiKhoan
    {
        [Key]  // <-- thêm dòng này để EF Core biết đây là primary key
        public string MANV { get; set; }

        public string MATKHAU { get; set; } // lưu hash
        public string VAITRO { get; set; }
    }
}
