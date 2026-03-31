namespace SchoolEquipmentManagement.Web.ViewModels.Users
{
    public class UserListItemViewModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string RoleDisplayName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LastSignInAt { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public bool IsLockedOut => LockoutEndUtc.HasValue && LockoutEndUtc.Value > DateTime.UtcNow;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
