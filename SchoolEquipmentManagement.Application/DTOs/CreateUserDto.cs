using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public bool TwoFactorEnabled { get; set; }
        public string PerformedByUserName { get; set; } = string.Empty;
    }
}
