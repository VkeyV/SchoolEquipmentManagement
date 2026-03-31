using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;


namespace SchoolEquipmentManagement.Application.Services
{
    public class EquipmentHistoryService : IEquipmentHistoryService
    {
        private readonly IEquipmentHistoryRepository _historyRepository;

        public EquipmentHistoryService(IEquipmentHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task AddHistoryRecordAsync(
            int equipmentId,
            HistoryActionType actionType,
            string changedBy,
            string? changedField = null,
            string? oldValue = null,
            string? newValue = null,
            string? comment = null)
        {
            var historyEntry = new EquipmentHistory(
                equipmentId,
                actionType,
                changedBy,
                changedField,
                oldValue,
                newValue,
                comment);

            await _historyRepository.AddAsync(historyEntry);
            await _historyRepository.SaveChangesAsync();
        }

        public async Task AddHistoryRecordsAsync(IEnumerable<HistoryRecordRequest> records)
        {
            var historyEntries = records
                .Select(record => new EquipmentHistory(
                    record.EquipmentId,
                    record.ActionType,
                    record.ChangedBy,
                    record.ChangedField,
                    record.OldValue,
                    record.NewValue,
                    record.Comment))
                .ToList();

            if (historyEntries.Count == 0)
            {
                return;
            }

            await _historyRepository.AddRangeAsync(historyEntries);
            await _historyRepository.SaveChangesAsync();
        }
    }
}
