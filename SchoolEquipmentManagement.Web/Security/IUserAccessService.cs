using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Web.Security
{
    public interface IUserAccessService
    {
        bool IsAuthenticated { get; }
        string CurrentUserName { get; }
        string CurrentDisplayName { get; }
        UserRole CurrentRole { get; }
        string CurrentRoleDisplayName { get; }
        bool HasPermission(ModulePermission permission);
    }
}
