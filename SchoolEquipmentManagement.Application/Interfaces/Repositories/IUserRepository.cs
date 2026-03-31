using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<List<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser?> GetByIdAsync(int id);
        Task<ApplicationUser?> GetByNormalizedUserNameAsync(string normalizedUserName);
        Task<ApplicationUser?> GetByNormalizedEmailAsync(string normalizedEmail);
        Task<bool> ExistsByNormalizedUserNameAsync(string normalizedUserName);
        Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail);
        Task<int> CountActiveAdministratorsAsync();
        Task AddAsync(ApplicationUser user);
        Task UpdateAsync(ApplicationUser user);
        Task SaveChangesAsync();
    }
}
