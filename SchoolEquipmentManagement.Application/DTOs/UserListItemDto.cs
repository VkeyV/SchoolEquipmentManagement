using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class UserListItemDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LastSignInAt { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
