using Microsoft.AspNetCore.Authorization;

namespace SchoolEquipmentManagement.Web.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class PermissionAuthorizeAttribute : AuthorizeAttribute
    {
        public PermissionAuthorizeAttribute(ModulePermission permission)
        {
            Permission = permission;
            Policy = PermissionPolicyNames.For(permission);
        }

        public ModulePermission Permission { get; }
    }
}
