using Microsoft.EntityFrameworkCore;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Infrastructure.Data;

namespace SchoolEquipmentManagement.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.UserName)
                .ToListAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ApplicationUser?> GetByNormalizedUserNameAsync(string normalizedUserName)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName);
        }

        public async Task<ApplicationUser?> GetByNormalizedEmailAsync(string normalizedEmail)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);
        }

        public async Task<bool> ExistsByNormalizedUserNameAsync(string normalizedUserName)
        {
            return await _context.Users.AnyAsync(x => x.NormalizedUserName == normalizedUserName);
        }

        public async Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail)
        {
            return await _context.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail);
        }

        public async Task<int> CountActiveAdministratorsAsync()
        {
            return await _context.Users.CountAsync(x => x.IsActive && x.Role == UserRole.Administrator);
        }

        public async Task AddAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
