using SchoolEquipmentManagement.Domain.Enums;
namespace SchoolEquipmentManagement.Application.Interfaces
{
    public interface IEquipmentHistoryService
    {
        Task AddHistoryRecordAsync(
            int equipmentId,
            HistoryActionType actionType,
            string changedBy,
            string? changedField = null,
            string? oldValue = null,
            string? newValue = null,
            string? comment = null);

        Task AddHistoryRecordsAsync(IEnumerable<HistoryRecordRequest> records);
    }

    public record HistoryRecordRequest(
        int EquipmentId,
        HistoryActionType ActionType,
        string ChangedBy,
        string? ChangedField = null,
        string? OldValue = null,
        string? NewValue = null,
        string? Comment = null);
}
