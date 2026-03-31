using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Web.Security
{
    public sealed class ModulePermissionAuthorizationHandler
        : AuthorizationHandler<ModulePermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ModulePermissionRequirement requirement)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Task.CompletedTask;
            }

            var rawRole = context.User.FindFirstValue(ClaimTypes.Role);
            if (Enum.TryParse<UserRole>(rawRole, ignoreCase: true, out var role) &&
                UserPermissionMatrix.HasPermission(role, requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
