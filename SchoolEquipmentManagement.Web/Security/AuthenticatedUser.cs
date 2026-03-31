using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Web.Security
{
    public sealed record AuthenticatedUser(
        int Id,
        string UserName,
        string DisplayName,
        UserRole Role);
}
