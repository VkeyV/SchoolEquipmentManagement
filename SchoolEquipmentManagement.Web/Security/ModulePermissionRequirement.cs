using Microsoft.AspNetCore.Authorization;

namespace SchoolEquipmentManagement.Web.Security
{
    public sealed class ModulePermissionRequirement : IAuthorizationRequirement
    {
        public ModulePermissionRequirement(ModulePermission permission)
        {
            Permission = permission;
        }

        public ModulePermission Permission { get; }
    }
}
