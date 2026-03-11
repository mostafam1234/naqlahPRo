using Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Presentaion.Authorization
{
    /// <summary>
    /// Use on controllers or actions to require a specific permission (stored in NA_RoleClaims, ClaimType = "Permission").
    /// Users with FullControl bypass the check and are always allowed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
    {
        public const string PermissionClaimType = "Permission";

        public string Permission { get; }

        public RequirePermissionAttribute(string permission)
        {
            Permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (string.IsNullOrEmpty(Permission))
            {
                context.Result = new ForbidResult();
                return;
            }

            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var hasFullControl = user.Claims.Any(c =>
                string.Equals(c.Type, PermissionClaimType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(c.Value, PermissionNames.FullControl, StringComparison.Ordinal));

            if (hasFullControl)
                return;

            var hasPermission = user.Claims.Any(c =>
                string.Equals(c.Type, PermissionClaimType, StringComparison.OrdinalIgnoreCase)
                && string.Equals(c.Value, Permission, StringComparison.Ordinal));

            if (!hasPermission)
                context.Result = new ForbidResult();
        }
    }
}
