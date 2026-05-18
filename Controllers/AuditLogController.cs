using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDH.Authorization;
using QLDH.Models;
using System.Text.Json;
using Oracle.ManagedDataAccess.Client;

namespace QLDH.Controllers
{
    /// <summary>
    /// Controller quản lý nhật ký hệ thống (Audit Log)
    /// Chỉ cho phép vai trò Quản lý truy cập
    /// </summary>
    [RequireLogin]
    public class AuditLogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthorizationService _authService;

        public AuditLogController(ApplicationDbContext context, AuthorizationService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Danh sách nhật ký hệ thống với phân trang và lọc
        /// </summary>
        [RoleAuthorize(Roles.QuanLy)]
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? filterAction,
            string? filterEntity,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 20)
        {
            // Kiểm tra quyền
            if (!_authService.HasPermission(Permissions.AuditLog_View))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var query = _context.AuditLogs.Include(a => a.NhanVien).AsQueryable();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a =>
                    (a.MaNV != null && a.MaNV.ToLower().Contains(searchTerm)) ||
                    (a.NhanVien != null && a.NhanVien.TENNV != null && a.NhanVien.TENNV.ToLower().Contains(searchTerm)) ||
                    (a.EntityId != null && a.EntityId.ToLower().Contains(searchTerm)) ||
                    (a.Description != null && a.Description.ToLower().Contains(searchTerm)));
            }

            // Lọc theo loại hành động
            if (!string.IsNullOrEmpty(filterAction))
            {
                query = query.Where(a => a.Action == filterAction);
            }

            // Lọc theo entity
            if (!string.IsNullOrEmpty(filterEntity))
            {
                query = query.Where(a => a.EntityType == filterEntity);
            }

            // Lọc theo khoảng thời gian
            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Thêm 1 ngày để bao gồm cả ngày cuối
                query = query.Where(a => a.Timestamp < toDate.Value.AddDays(1));
            }

            // Sắp xếp theo thời gian giảm dần (mới nhất trước)
            query = query.OrderByDescending(a => a.Timestamp);

            // Tính tổng số bản ghi
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Phân trang
            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy danh sách các loại action và entity để hiển thị trong dropdown
            var distinctActions = await _context.AuditLogs
                .Select(a => a.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            var distinctEntities = await _context.AuditLogs
                .Select(a => a.EntityType)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync();

            // Truyền dữ liệu vào ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            ViewBag.SearchTerm = searchTerm;
            ViewBag.FilterAction = filterAction;
            ViewBag.FilterEntity = filterEntity;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            ViewBag.DistinctActions = distinctActions;
            ViewBag.DistinctEntities = distinctEntities;

            return View(logs);
        }

        /// <summary>
        /// Chi tiết một bản ghi audit log
        /// </summary>
        [RoleAuthorize(Roles.QuanLy)]
        public async Task<IActionResult> Details(int id)
        {
            if (!_authService.HasPermission(Permissions.AuditLog_View))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bản ghi nhật ký.";
                return RedirectToAction(nameof(Index));
            }

            return View(log);
        }

        /// <summary>
        /// Xóa các bản ghi audit log cũ hơn một khoảng thời gian
        /// </summary>
        [HttpPost]
        [RoleAuthorize(Roles.QuanLy)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearOldLogs(int daysToKeep = 90)
        {
            if (!_authService.HasPermission(Permissions.AuditLog_View))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldLogs = await _context.AuditLogs
                    .Where(a => a.Timestamp < cutoffDate)
                    .ToListAsync();

                var deletedCount = oldLogs.Count;

                if (deletedCount > 0)
                {
                    _context.AuditLogs.RemoveRange(oldLogs);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Đã xóa {deletedCount} bản ghi nhật ký cũ hơn {daysToKeep} ngày.";
                }
                else
                {
                    TempData["InfoMessage"] = $"Không có bản ghi nào cũ hơn {daysToKeep} ngày.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa nhật ký: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Export audit logs ra file CSV
        /// </summary>
        [RoleAuthorize(Roles.QuanLy)]
        public async Task<IActionResult> ExportCsv(
            string? searchTerm,
            string? filterAction,
            string? filterEntity,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!_authService.HasPermission(Permissions.AuditLog_View))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var query = _context.AuditLogs.AsQueryable();

            // Áp dụng các bộ lọc
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(a =>
                    (a.UserName != null && a.UserName.ToLower().Contains(searchTerm)) ||
                    (a.EntityId != null && a.EntityId.ToLower().Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(filterAction))
            {
                query = query.Where(a => a.Action == filterAction);
            }

            if (!string.IsNullOrEmpty(filterEntity))
            {
                query = query.Where(a => a.EntityType == filterEntity);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(a => a.Timestamp < toDate.Value.AddDays(1));
            }

            var logs = await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            // Tạo CSV content
            var csvBuilder = new System.Text.StringBuilder();
            csvBuilder.AppendLine("ID,Thời gian,Người dùng,Hành động,Đối tượng,ID Đối tượng,Mô tả,IP");

            foreach (var log in logs)
            {
                csvBuilder.AppendLine($"{log.Id},{log.Timestamp:yyyy-MM-dd HH:mm:ss}," +
                    $"\"{log.UserName}\",\"{log.Action}\",\"{log.EntityType}\"," +
                    $"\"{log.EntityId}\",\"{log.Description?.Replace("\"", "\"\"")}\",\"{log.IpAddress}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString());
            var fileName = $"AuditLog_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }

        /// <summary>
        /// Thống kê nhật ký theo loại hành động
        /// </summary>
        [RoleAuthorize(Roles.QuanLy)]
        public async Task<IActionResult> Statistics()
        {
            if (!_authService.HasPermission(Permissions.AuditLog_View))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Thống kê theo hành động
            var actionStats = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Thống kê theo entity
            var entityStats = await _context.AuditLogs
                .GroupBy(a => a.EntityType)
                .Select(g => new { Entity = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Thống kê theo người dùng
            var userStats = await _context.AuditLogs
                .GroupBy(a => a.UserName)
                .Select(g => new { User = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Thống kê theo ngày (7 ngày gần nhất)
            var last7Days = DateTime.UtcNow.AddDays(-7);
            var dailyStats = await _context.AuditLogs
                .Where(a => a.Timestamp >= last7Days)
                .GroupBy(a => a.Timestamp.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            ViewBag.ActionStats = actionStats;
            ViewBag.EntityStats = entityStats;
            ViewBag.UserStats = userStats;
            ViewBag.DailyStats = dailyStats;

            return View();
        }

        /// <summary>
        /// Phục hồi dữ liệu từ audit log
        /// </summary>
        [HttpPost]
        [RoleAuthorize(Roles.QuanLy)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreData(int logId)
        {
            if (!_authService.HasPermission(Permissions.AuditLog_View))
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            try
            {
                var log = await _context.AuditLogs.FindAsync(logId);
                if (log == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bản ghi nhật ký.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrEmpty(log.OldValues))
                {
                    TempData["ErrorMessage"] = "Không có dữ liệu cũ để phục hồi.";
                    return RedirectToAction(nameof(Index));
                }

                var tableName = log.EntityType;
                var entityId = log.EntityId;
                var action = log.Action;

                // Parse JSON từ OldValues
                var jsonDoc = JsonDocument.Parse(log.OldValues);
                var oldData = new Dictionary<string, object?>();
                
                foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
                    oldData[property.Name] = GetValueFromJsonElement(property.Value);
                }
                
                if (oldData == null || !oldData.Any())
                {
                    TempData["ErrorMessage"] = "Dữ liệu cũ không hợp lệ.";
                    return RedirectToAction(nameof(Index));
                }

                using (var connection = _context.Database.GetDbConnection())
                {
                    await connection.OpenAsync();
                    using var transaction = await connection.BeginTransactionAsync();

                    try
                    {
                        if (action == "DELETE")
                        {
                            // Khôi phục bản ghi đã xóa - INSERT lại
                            var columns = string.Join(", ", oldData.Keys);
                            var values = string.Join(", ", oldData.Keys.Select(k => $":{k}"));
                            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

                            using var oracleCmd = new OracleCommand(sql, (OracleConnection)connection);
                            oracleCmd.Transaction = (OracleTransaction)transaction;

                            foreach (var kvp in oldData)
                            {
                                var param = new OracleParameter(kvp.Key, kvp.Value ?? DBNull.Value);
                                oracleCmd.Parameters.Add(param);
                            }

                            await oracleCmd.ExecuteNonQueryAsync();
                            TempData["SuccessMessage"] = $"Đã khôi phục bản ghi đã xóa: {tableName} (ID: {entityId})";
                        }
                        else if (action == "UPDATE")
                        {
                            // Hoàn tác thay đổi - UPDATE về giá trị cũ
                            var setPairs = string.Join(", ", oldData.Keys.Select(k => $"{k} = :{k}"));
                            
                            // Tìm primary key column
                            string pkColumn = GetPrimaryKeyColumn(tableName);
                            var sql = $"UPDATE {tableName} SET {setPairs} WHERE {pkColumn} = :PK_VALUE";

                            using var oracleCmd = new OracleCommand(sql, (OracleConnection)connection);
                            oracleCmd.Transaction = (OracleTransaction)transaction;

                            foreach (var kvp in oldData)
                            {
                                var param = new OracleParameter(kvp.Key, kvp.Value ?? DBNull.Value);
                                oracleCmd.Parameters.Add(param);
                            }

                            // Primary key parameter
                            var pkParam = new OracleParameter("PK_VALUE", entityId);
                            oracleCmd.Parameters.Add(pkParam);

                            var rowsAffected = await oracleCmd.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                TempData["SuccessMessage"] = $"Đã hoàn tác thay đổi: {tableName} (ID: {entityId})";
                            }
                            else
                            {
                                TempData["ErrorMessage"] = $"Không tìm thấy bản ghi để hoàn tác (có thể đã bị xóa).";
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Không hỗ trợ phục hồi cho hành động: {action}";
                            return RedirectToAction(nameof(Index));
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = $"Lỗi khi phục hồi dữ liệu: {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Lấy tên cột primary key của bảng
        /// </summary>
        private string GetPrimaryKeyColumn(string tableName)
        {
            return tableName switch
            {
                "NHANVIEN" => "MANV",
                "KHACHHANG" => "MAKH",
                "DONHANG" => "MADH",
                "SANPHAM" => "MASP",
                "HOADON" => "MAHD",
                "LOAISP" => "MALOAISP",
                "LOAIKH" => "MALOAIKH",
                "NCC" => "MANCC",
                "TAIKHOAN" => "MANV",
                _ => "ID"
            };
        }

        /// <summary>
        /// Convert JsonElement sang giá trị phù hợp cho Oracle
        /// </summary>
        private object? GetValueFromJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => null,
                _ => element.ToString()
            };
        }
    }
}

