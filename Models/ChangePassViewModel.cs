using System.ComponentModel.DataAnnotations;

namespace QLDH.Models
{
    public class ChangePassViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã nhân viên.")]
        [Display(Name = "Mã nhân viên")]
        public string Manv { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{8,}$",
            ErrorMessage = "Mật khẩu phải tối thiểu 8 ký tự, gồm chữ hoa, chữ thường và ký tự đặc biệt.")]
        [Display(Name = "Mật khẩu mới")]
        public string MatkhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare("MatkhauMoi", ErrorMessage = "Mật khẩu nhập lại không khớp.")]
        [Display(Name = "Nhập lại mật khẩu mới")]
        public string NhapLaiMatkhauMoi { get; set; }
    }
}
