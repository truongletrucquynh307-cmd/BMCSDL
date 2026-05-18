using Microsoft.AspNetCore.Http;
using QLDH.Models;
using System.Text.Json;

namespace QLDH.Services
{
    /// <summary>
    /// Service ghi nhật ký hoạt động hệ thống
    /// </summary>
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Ghi log hoạt động
        /// </summary>
        public async Task LogAsync(
            string action,
            string entityType,
            string? entityId = null,
            object? oldValues = null,
            object? newValues = null,
            string? description = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var maNV = httpContext?.Session.GetString("MaNV");
                var ipAddress = GetClientIpAddress(httpContext);

                var log = new AuditLog
                {
                    Timestamp = DateTime.Now,
                    MaNV = maNV,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    IpAddress = ipAddress,
                    Description = description
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi không nên ảnh hưởng đến luồng chính
                Console.WriteLine($"Lỗi ghi audit log: {ex.Message}");
            }
        }

        /// <summary>
        /// Ghi log đồng bộ
        /// </summary>
        public void Log(
            string action,
            string entityType,
            string? entityId = null,
            object? oldValues = null,
            object? newValues = null,
            string? description = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var maNV = httpContext?.Session.GetString("MaNV");
                var ipAddress = GetClientIpAddress(httpContext);

                var log = new AuditLog
                {
                    Timestamp = DateTime.Now,
                    MaNV = maNV,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                    IpAddress = ipAddress,
                    Description = description
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ghi audit log: {ex.Message}");
            }
        }

        /// <summary>
        /// Ghi log đăng nhập
        /// </summary>
        public void LogLogin(string maNV, string tenNV, bool success, string? reason = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);

                var log = new AuditLog
                {
                    Timestamp = DateTime.Now,
                    MaNV = maNV,
                    Action = success ? "Login" : "LoginFailed",
                    EntityType = "TaiKhoan",
                    EntityId = maNV,
                    IpAddress = ipAddress,
                    Description = success 
                        ? $"Đăng nhập thành công từ {ipAddress}" 
                        : $"Đăng nhập thất bại: {reason}"
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ghi audit log login: {ex.Message}");
            }
        }

        /// <summary>
        /// Ghi log đăng xuất
        /// </summary>
        public void LogLogout()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var maNV = httpContext?.Session.GetString("MaNV");
                var ipAddress = GetClientIpAddress(httpContext);

                if (!string.IsNullOrEmpty(maNV))
                {
                    var log = new AuditLog
                    {
                        Timestamp = DateTime.Now,
                        MaNV = maNV,
                        Action = "Logout",
                        EntityType = "TaiKhoan",
                        EntityId = maNV,
                        IpAddress = ipAddress,
                        Description = $"Đăng xuất từ {ipAddress}"
                    };

                    _context.AuditLogs.Add(log);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ghi audit log logout: {ex.Message}");
            }
        }

        /// <summary>
        /// Ghi log đổi mật khẩu
        /// </summary>
        public void LogChangePassword(string maNV, bool success, string? reason = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var ipAddress = GetClientIpAddress(httpContext);

                var log = new AuditLog
                {
                    Timestamp = DateTime.Now,
                    MaNV = maNV,
                    Action = "ChangePassword",
                    EntityType = "TaiKhoan",
                    EntityId = maNV,
                    IpAddress = ipAddress,
                    Description = success 
                        ? $"Đổi mật khẩu thành công" 
                        : $"Đổi mật khẩu thất bại: {reason}"
                };

                _context.AuditLogs.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi ghi audit log change password: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy địa chỉ IP của client
        /// </summary>
        private string? GetClientIpAddress(HttpContext? httpContext)
        {
            if (httpContext == null) return null;

            // Kiểm tra header X-Forwarded-For (nếu dùng reverse proxy)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            // Nếu không, lấy từ RemoteIpAddress
            return httpContext.Connection.RemoteIpAddress?.ToString();
        }
    }

    /// <summary>
    /// Constants cho Action types
    /// </summary>
    public static class AuditActions
    {
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Delete = "Delete";
        public const string View = "View";
        public const string Export = "Export";
        public const string Login = "Login";
        public const string LoginFailed = "LoginFailed";
        public const string Logout = "Logout";
        public const string ChangePassword = "ChangePassword";
    }
}
