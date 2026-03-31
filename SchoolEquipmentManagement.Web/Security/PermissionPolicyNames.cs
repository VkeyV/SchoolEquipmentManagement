namespace SchoolEquipmentManagement.Web.Security
{
    public static class PermissionPolicyNames
    {
        private const string Prefix = "Permission:";

        public static string For(ModulePermission permission) => $"{Prefix}{permission}";
    }
}
