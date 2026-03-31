using SchoolEquipmentManagement.Domain.Enums;

namespace SchoolEquipmentManagement.Application.DTOs
{
    public class UserEditDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}
