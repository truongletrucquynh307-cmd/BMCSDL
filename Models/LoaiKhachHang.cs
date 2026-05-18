using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace QLDH.Models
{
    [Table("LOAIKH")] 
    public class LoaiKhachHang
    {
        [Key]
        [StringLength(8)]
        public string MALOAIKH { get; set; }

        [Required]
        [StringLength(50)]
        public string TENLOAIKH { get; set; }

        
    }
}
