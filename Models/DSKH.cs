using System.ComponentModel.DataAnnotations;

namespace QLDH.Models
{
    public class KhachHang
    {
        [Key]
        [Display(Name = "Mã khách hàng")]
        public string? MAKH { get; set; } 


        [Display(Name = "Tên khách hàng")]
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        public string TENKH { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        public string DIACHI { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SDT { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string EMAIL { get; set; }

        [Display(Name = "Loại khách hàng")]
        [Required(ErrorMessage = "Loại khách hàng không được để trống")]
        public string MALOAIKH { get; set; }
    }
}
