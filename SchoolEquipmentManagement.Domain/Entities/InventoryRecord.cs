using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class InventoryRecord : BaseEntity
    {
        public int InventorySessionId { get; private set; }
        public InventorySession InventorySession { get; private set; }

        public int EquipmentId { get; private set; }
        public Equipment Equipment { get; private set; }

        public int? ActualLocationId { get; private set; }
        public Location? ActualLocation { get; private set; }

        public bool IsFound { get; private set; }
        public string? ConditionComment { get; private set; }
        public DateTime CheckedAt { get; private set; }
        public string CheckedBy { get; private set; }

        private InventoryRecord()
        {
            InventorySession = null!;
            Equipment = null!;
            CheckedBy = null!;
        }

        public InventoryRecord(
            int inventorySessionId,
            int equipmentId,
            bool isFound,
            string checkedBy,
            int? actualLocationId = null,
            string? conditionComment = null)
        {
            if (inventorySessionId <= 0)
                throw new DomainException("Некорректный идентификатор сессии инвентаризации.");

            if (equipmentId <= 0)
                throw new DomainException("Некорректный идентификатор оборудования.");

            if (string.IsNullOrWhiteSpace(checkedBy))
                throw new DomainException("Не указан пользователь, выполнивший проверку.");

            InventorySessionId = inventorySessionId;
            EquipmentId = equipmentId;
            IsFound = isFound;
            ActualLocationId = actualLocationId;
            ConditionComment = string.IsNullOrWhiteSpace(conditionComment) ? null : conditionComment.Trim();
            CheckedBy = checkedBy.Trim();
            CheckedAt = DateTime.UtcNow;
        }

        public void UpdateResult(bool isFound, int? actualLocationId, string? conditionComment)
        {
            IsFound = isFound;
            ActualLocationId = actualLocationId;
            ConditionComment = string.IsNullOrWhiteSpace(conditionComment) ? null : conditionComment.Trim();
            CheckedAt = DateTime.UtcNow;
        }
    }
}
