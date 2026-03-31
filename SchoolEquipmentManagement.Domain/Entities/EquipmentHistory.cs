using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class EquipmentHistory : BaseEntity
    {
        public int EquipmentId { get; private set; }
        public Equipment Equipment { get; private set; }

        public HistoryActionType ActionType { get; private set; }
        public string? ChangedField { get; private set; }
        public string? OldValue { get; private set; }
        public string? NewValue { get; private set; }
        public string? Comment { get; private set; }
        public string ChangedBy { get; private set; }
        public DateTime ChangedAt { get; private set; }

        private EquipmentHistory()
        {
            Equipment = null!;
            ChangedBy = null!;
        }

        public EquipmentHistory(
            int equipmentId,
            HistoryActionType actionType,
            string changedBy,
            string? changedField = null,
            string? oldValue = null,
            string? newValue = null,
            string? comment = null)
        {
            if (equipmentId <= 0)
                throw new DomainException("Некорректный идентификатор оборудования для истории изменений.");

            if (string.IsNullOrWhiteSpace(changedBy))
                throw new DomainException("Не указан пользователь, выполнивший изменение.");

            EquipmentId = equipmentId;
            ActionType = actionType;
            ChangedField = Normalize(changedField);
            OldValue = Normalize(oldValue);
            NewValue = Normalize(newValue);
            Comment = Normalize(comment);
            ChangedBy = changedBy.Trim();
            ChangedAt = DateTime.UtcNow;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
