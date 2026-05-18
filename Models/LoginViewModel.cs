using System.ComponentModel.DataAnnotations;

namespace  QLDH.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã nhân viên.")]
        public string Manv { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Matkhau { get; set; }
    }
}
