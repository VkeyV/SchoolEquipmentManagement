using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Web.Security;

namespace SchoolEquipmentManagement.Tests.TestSupport
{
    internal sealed class FakeUserAccessService : IUserAccessService
    {
        private readonly HashSet<ModulePermission> _permissions;

        public FakeUserAccessService(
            params ModulePermission[] permissions)
        {
            _permissions = permissions.ToHashSet();
        }

        public bool IsAuthenticated => true;

        public string CurrentUserName => "TestUser";

        public string CurrentDisplayName => "TestUser";

        public UserRole CurrentRole => UserRole.Administrator;

        public string CurrentRoleDisplayName => UserPermissionMatrix.GetRoleDisplayName(CurrentRole);

        public bool HasPermission(ModulePermission permission) => _permissions.Contains(permission);
    }
}
