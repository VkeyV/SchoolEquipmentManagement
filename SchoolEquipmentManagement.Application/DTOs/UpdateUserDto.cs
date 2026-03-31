using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string? NewPassword { get; set; }
        public string PerformedByUserName { get; set; } = string.Empty;
    }
}
