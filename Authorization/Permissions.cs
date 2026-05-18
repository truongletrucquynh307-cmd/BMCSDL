namespace QLDH.Authorization
{
    /// <summary>
    /// Định nghĩa các vai trò trong hệ thống
    /// </summary>
    public static class Roles
    {
        public const string QuanLy = "Quản lý";
        public const string BanHang = "Bán hàng";
        public const string KhoHang = "Kho hàng";

        public static readonly string[] AllRoles = { QuanLy, BanHang, KhoHang };

        /// <summary>
        /// Kiểm tra vai trò có hợp lệ không
        /// </summary>
        public static bool IsValidRole(string? role)
        {
            if (string.IsNullOrEmpty(role)) return false;
            return role == QuanLy || role == BanHang || role == KhoHang;
        }
    }

    /// <summary>
    /// Định nghĩa các quyền trong hệ thống
    /// </summary>
    public static class Permissions
    {
        // Quản lý người dùng/nhân viên
        public const string NhanVien_View = "NhanVien.View";
        public const string NhanVien_Create = "NhanVien.Create";
        public const string NhanVien_Edit = "NhanVien.Edit";
        public const string NhanVien_Delete = "NhanVien.Delete";
        public const string NhanVien_EditSelf = "NhanVien.EditSelf";

        // Quản lý sản phẩm
        public const string SanPham_View = "SanPham.View";
        public const string SanPham_Create = "SanPham.Create";
        public const string SanPham_Edit = "SanPham.Edit";
        public const string SanPham_Delete = "SanPham.Delete";

        // Quản lý khách hàng
        public const string KhachHang_View = "KhachHang.View";
        public const string KhachHang_Create = "KhachHang.Create";
        public const string KhachHang_Edit = "KhachHang.Edit";
        public const string KhachHang_Delete = "KhachHang.Delete";

        // Quản lý đơn hàng
        public const string DonHang_View = "DonHang.View";
        public const string DonHang_ViewAll = "DonHang.ViewAll";
        public const string DonHang_ViewOwn = "DonHang.ViewOwn";
        public const string DonHang_Create = "DonHang.Create";
        public const string DonHang_Edit = "DonHang.Edit";
        public const string DonHang_EditOwn = "DonHang.EditOwn";
        public const string DonHang_Delete = "DonHang.Delete";
        public const string DonHang_UpdateStatus = "DonHang.UpdateStatus";

        // Quản lý hóa đơn
        public const string HoaDon_View = "HoaDon.View";
        public const string HoaDon_Create = "HoaDon.Create";
        public const string HoaDon_Edit = "HoaDon.Edit";
        public const string HoaDon_Delete = "HoaDon.Delete";

        // Quản lý nhà cung cấp
        public const string NhaCungCap_View = "NhaCungCap.View";
        public const string NhaCungCap_Create = "NhaCungCap.Create";
        public const string NhaCungCap_Edit = "NhaCungCap.Edit";
        public const string NhaCungCap_Delete = "NhaCungCap.Delete";

        // Quản lý loại sản phẩm
        public const string LoaiSanPham_View = "LoaiSanPham.View";
        public const string LoaiSanPham_Create = "LoaiSanPham.Create";
        public const string LoaiSanPham_Edit = "LoaiSanPham.Edit";
        public const string LoaiSanPham_Delete = "LoaiSanPham.Delete";

        // Audit Log
        public const string AuditLog_View = "AuditLog.View";
    }

    /// <summary>
    /// Ma trận phân quyền - Mapping giữa Role và Permissions
    /// </summary>
    public static class PermissionMatrix
    {
        /// <summary>
        /// Lấy danh sách quyền theo vai trò
        /// </summary>
        public static string[] GetPermissions(string? role)
        {
            return role switch
            {
                Roles.QuanLy => GetQuanLyPermissions(),
                Roles.BanHang => GetBanHangPermissions(),
                Roles.KhoHang => GetKhoHangPermissions(),
                _ => Array.Empty<string>()
            };
        }

        private static string[] GetQuanLyPermissions()
        {
            return new[]
            {
                // Full quyền nhân viên
                Permissions.NhanVien_View, Permissions.NhanVien_Create,
                Permissions.NhanVien_Edit, Permissions.NhanVien_Delete,
                
                // Full quyền sản phẩm
                Permissions.SanPham_View, Permissions.SanPham_Create,
                Permissions.SanPham_Edit, Permissions.SanPham_Delete,
                
                // Full quyền khách hàng
                Permissions.KhachHang_View, Permissions.KhachHang_Create,
                Permissions.KhachHang_Edit, Permissions.KhachHang_Delete,
                
                // Full quyền đơn hàng
                Permissions.DonHang_View, Permissions.DonHang_ViewAll,
                Permissions.DonHang_Create, Permissions.DonHang_Edit,
                Permissions.DonHang_Delete, Permissions.DonHang_UpdateStatus,
                
                // Full quyền hóa đơn
                Permissions.HoaDon_View, Permissions.HoaDon_Create,
                Permissions.HoaDon_Edit, Permissions.HoaDon_Delete,
                
                // Full quyền nhà cung cấp
                Permissions.NhaCungCap_View, Permissions.NhaCungCap_Create,
                Permissions.NhaCungCap_Edit, Permissions.NhaCungCap_Delete,
                
                // Full quyền loại sản phẩm
                Permissions.LoaiSanPham_View, Permissions.LoaiSanPham_Create,
                Permissions.LoaiSanPham_Edit, Permissions.LoaiSanPham_Delete,
                
                // Xem audit log
                Permissions.AuditLog_View
            };
        }

        private static string[] GetBanHangPermissions()
        {
            return new[]
            {
                // Chỉ sửa thông tin cá nhân
                Permissions.NhanVien_View, Permissions.NhanVien_EditSelf,
                
                // Full quyền khách hàng
                Permissions.KhachHang_View, Permissions.KhachHang_Create,
                Permissions.KhachHang_Edit, Permissions.KhachHang_Delete,
                
                // Đơn hàng: Thêm và chỉ sửa đơn của mình
                Permissions.DonHang_View, Permissions.DonHang_ViewOwn,
                Permissions.DonHang_Create, Permissions.DonHang_EditOwn,
                
                // Hóa đơn: Chỉ sửa
                Permissions.HoaDon_View, Permissions.HoaDon_Edit,
                
                // Full quyền nhà cung cấp
                //Permissions.NhaCungCap_View, Permissions.NhaCungCap_Create,
                //Permissions.NhaCungCap_Edit, Permissions.NhaCungCap_Delete,
                
                // Xem sản phẩm và loại sản phẩm
                Permissions.SanPham_View, Permissions.LoaiSanPham_View
            };
        }

        private static string[] GetKhoHangPermissions()
        {
            return new[]
            {
                // Chỉ sửa thông tin cá nhân
                Permissions.NhanVien_View, Permissions.NhanVien_EditSelf,
                
                // Full quyền sản phẩm
                Permissions.SanPham_View, Permissions.SanPham_Create,
                Permissions.SanPham_Edit, Permissions.SanPham_Delete,
                
                // Full quyền loại sản phẩm
                Permissions.LoaiSanPham_View, Permissions.LoaiSanPham_Create,
                Permissions.LoaiSanPham_Edit, Permissions.LoaiSanPham_Delete,
                
               // Full quyền nhà cung cấp
                Permissions.NhaCungCap_View, Permissions.NhaCungCap_Create,
                Permissions.NhaCungCap_Edit, Permissions.NhaCungCap_Delete,
                
                // Đơn hàng: Xem tất cả và cập nhật trạng thái
                Permissions.DonHang_View, Permissions.DonHang_ViewAll,
                Permissions.DonHang_UpdateStatus
            };
        }

        /// <summary>
        /// Kiểm tra role có quyền không
        /// </summary>
        public static bool HasPermission(string? role, string permission)
        {
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(permission))
                return false;

            var permissions = GetPermissions(role);
            return permissions.Contains(permission);
        }
    }
}
