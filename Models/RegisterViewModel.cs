using System.ComponentModel.DataAnnotations;

namespace QLDH.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã nhân viên")]
        public string Manv { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên nhân viên")]
        public string Tennv { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chức vụ")]
        [Display(Name = "Chức vụ")]
        public string ChucVu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Sdt { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{8,}$",
            ErrorMessage = "Mật khẩu phải tối thiểu 8 ký tự, gồm chữ hoa, chữ thường và ký tự đặc biệt")]
        public string Matkhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Matkhau", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string NhapLaiMatkhau { get; set; }
    }
}
