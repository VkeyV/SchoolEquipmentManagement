using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Web.Security
{
    public static class UserPermissionMatrix
    {
        public static bool HasPermission(UserRole role, ModulePermission permission)
        {
            return role switch
            {
                UserRole.Administrator => true,
                UserRole.Technician => permission is
                    ModulePermission.ViewEquipment or
                    ModulePermission.CreateEquipment or
                    ModulePermission.EditEquipment or
                    ModulePermission.ChangeEquipmentStatus or
                    ModulePermission.ChangeEquipmentLocation or
                    ModulePermission.WriteOffEquipment or
                    ModulePermission.ImportEquipment or
                    ModulePermission.ViewInventory or
                    ModulePermission.CreateInventorySession or
                    ModulePermission.ManageInventorySession or
                    ModulePermission.CheckInventory,
                UserRole.Responsible => permission is
                    ModulePermission.ViewEquipment or
                    ModulePermission.ViewInventory or
                    ModulePermission.CheckInventory,
                UserRole.Viewer => permission is
                    ModulePermission.ViewEquipment or
                    ModulePermission.ViewInventory,
                _ => false
            };
        }

        public static string GetRoleDisplayName(UserRole role)
        {
            return role switch
            {
                UserRole.Administrator => "Администратор",
                UserRole.Technician => "Техник",
                UserRole.Responsible => "Ответственный",
                UserRole.Viewer => "Наблюдатель",
                _ => "Пользователь"
            };
        }
    }
}
