using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;

namespace SchoolEquipmentManagement.Application.Services
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IDictionaryRepository _dictionaryRepository;

        public DictionaryService(IDictionaryRepository dictionaryRepository)
        {
            _dictionaryRepository = dictionaryRepository;
        }

        public async Task<List<LookupItemDto>> GetEquipmentTypesAsync()
        {
            var items = await _dictionaryRepository.GetEquipmentTypesAsync();

            return items.Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        public async Task<List<LookupItemDto>> GetEquipmentStatusesAsync()
        {
            var items = await _dictionaryRepository.GetEquipmentStatusesAsync();

            return items.Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        public async Task<List<LookupItemDto>> GetLocationsAsync()
        {
            var items = await _dictionaryRepository.GetLocationsAsync();

            return items.Select(x => new LookupItemDto
            {
                Id = x.Id,
                Name = x.GetDisplayName()
            }).ToList();
        }
    }
}
