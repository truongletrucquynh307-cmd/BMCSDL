using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QLDH.Authorization
{
    /// <summary>
    /// Attribute kiểm tra quyền truy cập dựa trên Role
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _allowedRoles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");
            var maNV = context.HttpContext.Session.GetString("MaNV");

            // Kiểm tra đã đăng nhập chưa
            if (string.IsNullOrEmpty(maNV))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Kiểm tra role
            if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(vaiTro))
            {
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/AccessDenied.cshtml"
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Attribute kiểm tra quyền truy cập dựa trên Permission
    /// Hỗ trợ nhiều permissions (OR logic - chỉ cần 1 trong số các permission)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _permissions;

        public PermissionAuthorizeAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var vaiTro = context.HttpContext.Session.GetString("VaiTro");
            var maNV = context.HttpContext.Session.GetString("MaNV");

            // Kiểm tra đã đăng nhập chưa
            if (string.IsNullOrEmpty(maNV))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Kiểm tra permission (OR logic - chỉ cần có 1 trong các permission)
            bool hasAnyPermission = _permissions.Any(p => PermissionMatrix.HasPermission(vaiTro, p));
            if (!hasAnyPermission)
            {
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/AccessDenied.cshtml"
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Attribute yêu cầu đăng nhập
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var maNV = context.HttpContext.Session.GetString("MaNV");

            if (string.IsNullOrEmpty(maNV))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
