using SchoolEquipmentManagement.Application.DTOs;

namespace SchoolEquipmentManagement.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<IReadOnlyList<UserListItemDto>> GetUsersAsync();
        Task<UserEditDto?> GetUserForEditAsync(int id);
        Task<int> CreateUserAsync(CreateUserDto dto);
        Task UpdateUserAsync(UpdateUserDto dto);
    }
}
