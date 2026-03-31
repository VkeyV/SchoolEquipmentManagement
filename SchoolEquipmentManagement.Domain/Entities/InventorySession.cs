using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class InventorySession : AuditableEntity
    {
        public string Name { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public InventorySessionStatus Status { get; private set; }
        public string CreatedBy { get; private set; }

        private readonly List<InventoryRecord> _records = new();
        public IReadOnlyCollection<InventoryRecord> Records => _records.AsReadOnly();

        private InventorySession()
        {
            Name = null!;
            CreatedBy = null!;
        }

        public InventorySession(string name, DateTime startDate, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Наименование инвентаризации не может быть пустым.");

            if (string.IsNullOrWhiteSpace(createdBy))
                throw new DomainException("Не указан пользователь, создавший инвентаризацию.");

            Name = name.Trim();
            StartDate = startDate;
            CreatedBy = createdBy.Trim();
            Status = InventorySessionStatus.Draft;
        }

        public void Start()
        {
            if (Status != InventorySessionStatus.Draft)
                throw new DomainException("Начать можно только инвентаризацию в статусе черновика.");

            Status = InventorySessionStatus.InProgress;
            MarkAsUpdated();
        }

        public void Complete(DateTime endDate)
        {
            if (Status != InventorySessionStatus.InProgress)
                throw new DomainException("Завершить можно только инвентаризацию в процессе выполнения.");

            if (endDate < StartDate)
                throw new DomainException("Дата завершения инвентаризации не может быть раньше даты начала.");

            EndDate = endDate;
            Status = InventorySessionStatus.Completed;
            MarkAsUpdated();
        }

        public void Cancel()
        {
            if (Status == InventorySessionStatus.Completed)
                throw new DomainException("Завершенную инвентаризацию нельзя отменить.");

            Status = InventorySessionStatus.Cancelled;
            MarkAsUpdated();
        }
    }
}
