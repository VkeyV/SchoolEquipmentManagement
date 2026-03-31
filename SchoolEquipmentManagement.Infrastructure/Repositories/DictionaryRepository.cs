using Microsoft.EntityFrameworkCore;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Infrastructure.Data;

namespace SchoolEquipmentManagement.Infrastructure.Repositories
{
    public class DictionaryRepository : IDictionaryRepository
    {
        private readonly ApplicationDbContext _context;

        public DictionaryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<EquipmentType>> GetEquipmentTypesAsync()
        {
            return await _context.EquipmentTypes
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<List<EquipmentStatus>> GetEquipmentStatusesAsync()
        {
            return await _context.EquipmentStatuses
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<List<Location>> GetLocationsAsync()
        {
            return await _context.Locations
                .OrderBy(x => x.Building)
                .ThenBy(x => x.Room)
                .ToListAsync();
        }
    }
}
