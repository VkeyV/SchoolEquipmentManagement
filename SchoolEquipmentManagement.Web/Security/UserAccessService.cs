using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Web.Security
{
    public sealed class UserAccessService : IUserAccessService
    {
        private const string DisplayNameClaimType = "display_name";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserAccessService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsAuthenticated => CurrentPrincipal.Identity?.IsAuthenticated ?? false;

        public string CurrentUserName =>
            CurrentPrincipal.Identity?.Name?.Trim() is { Length: > 0 } name
                ? name
                : "System";

        public string CurrentDisplayName =>
            CurrentPrincipal.FindFirstValue(DisplayNameClaimType)?.Trim() is { Length: > 0 } displayName
                ? displayName
                : CurrentUserName;

        public UserRole CurrentRole
        {
            get
            {
                var rawRole = CurrentPrincipal.FindFirstValue(ClaimTypes.Role);
                return Enum.TryParse<UserRole>(rawRole, ignoreCase: true, out var role)
                    ? role
                    : UserRole.Viewer;
            }
        }

        public string CurrentRoleDisplayName => UserPermissionMatrix.GetRoleDisplayName(CurrentRole);

        public bool HasPermission(ModulePermission permission) =>
            IsAuthenticated && UserPermissionMatrix.HasPermission(CurrentRole, permission);

        private ClaimsPrincipal CurrentPrincipal =>
            _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());
    }
}
