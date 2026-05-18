using Microsoft.AspNetCore.Http;

namespace QLDH.Authorization
{
    /// <summary>
    /// Service hỗ trợ kiểm tra quyền trong code
    /// </summary>
    public class AuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Lấy Mã NV đang đăng nhập
        /// </summary>
        public string? GetCurrentMaNV()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("MaNV");
        }

        /// <summary>
        /// Lấy Vai trò đang đăng nhập
        /// </summary>
        public string? GetCurrentVaiTro()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("VaiTro");
        }

        /// <summary>
        /// Lấy Tên NV đang đăng nhập
        /// </summary>
        public string? GetCurrentTenNV()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("TenNV");
        }

        /// <summary>
        /// Kiểm tra đã đăng nhập chưa
        /// </summary>
        public bool IsLoggedIn()
        {
            return !string.IsNullOrEmpty(GetCurrentMaNV());
        }

        /// <summary>
        /// Kiểm tra có phải Quản lý không
        /// </summary>
        public bool IsQuanLy()
        {
            return GetCurrentVaiTro() == Roles.QuanLy;
        }

        /// <summary>
        /// Kiểm tra có phải Nhân viên bán hàng không
        /// </summary>
        public bool IsBanHang()
        {
            return GetCurrentVaiTro() == Roles.BanHang;
        }

        /// <summary>
        /// Kiểm tra có phải Nhân viên kho không
        /// </summary>
        public bool IsKhoHang()
        {
            return GetCurrentVaiTro() == Roles.KhoHang;
        }

        /// <summary>
        /// Kiểm tra có quyền không
        /// </summary>
        public bool HasPermission(string permission)
        {
            return PermissionMatrix.HasPermission(GetCurrentVaiTro(), permission);
        }

        /// <summary>
        /// Kiểm tra có phải chủ sở hữu record không (dùng cho đơn hàng)
        /// </summary>
        public bool IsOwner(string? recordMaNV)
        {
            var currentMaNV = GetCurrentMaNV();
            return !string.IsNullOrEmpty(currentMaNV) && currentMaNV == recordMaNV;
        }

        /// <summary>
        /// Kiểm tra có thể xem đơn hàng không
        /// </summary>
        public bool CanViewDonHang(string? donHangMaNV)
        {
            // Quản lý: xem tất cả
            if (IsQuanLy()) return true;

            // Kho hàng: xem tất cả (để cập nhật trạng thái)
            if (IsKhoHang()) return true;

            // Bán hàng: chỉ xem đơn của mình
            if (IsBanHang()) return IsOwner(donHangMaNV);

            return false;
        }

        /// <summary>
        /// Kiểm tra có thể sửa đơn hàng không
        /// </summary>
        public bool CanEditDonHang(string? donHangMaNV)
        {
            // Quản lý: sửa tất cả
            if (IsQuanLy()) return true;

            // Bán hàng: chỉ sửa đơn của mình
            if (IsBanHang()) return IsOwner(donHangMaNV);

            return false;
        }

        /// <summary>
        /// Kiểm tra có thể cập nhật trạng thái đơn hàng không
        /// </summary>
        public bool CanUpdateDonHangStatus()
        {
            return IsQuanLy() || IsKhoHang();
        }

        /// <summary>
        /// Kiểm tra có thể sửa thông tin nhân viên không
        /// </summary>
        public bool CanEditNhanVien(string? nhanVienMaNV)
        {
            // Quản lý: sửa tất cả
            if (IsQuanLy()) return true;

            // Nhân viên thường: chỉ sửa thông tin của mình
            return IsOwner(nhanVienMaNV);
        }

        /// <summary>
        /// Lấy các trạng thái đơn hàng mà user có thể cập nhật
        /// </summary>
        public string[] GetAllowedDonHangStatuses()
        {
            if (IsQuanLy())
            {
                return new[] { "Đang xử lý", "Đã giao", "Đã hủy" };
            }

            if (IsKhoHang())
            {
                // Kho hàng chỉ được cập nhật thành "Đang giao" hoặc "Đã hủy"
                return new[] { "Đang giao", "Đã hủy" };
            }

            if (IsBanHang())
            {
                return new[] { "Đang xử lý" };
            }

            return Array.Empty<string>();
        }
    }
}
