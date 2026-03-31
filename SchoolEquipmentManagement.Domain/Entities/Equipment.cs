using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class Equipment : AuditableEntity
    {
        public string InventoryNumber { get; private set; }
        public string Name { get; private set; }
        public string? SerialNumber { get; private set; }
        public string? Model { get; private set; }
        public string? Manufacturer { get; private set; }
        public DateTime? PurchaseDate { get; private set; }
        public DateTime? CommissioningDate { get; private set; }
        public DateTime? WarrantyEndDate { get; private set; }
        public string? ResponsiblePerson { get; private set; }
        public string? Notes { get; private set; }

        public int EquipmentTypeId { get; private set; }
        public EquipmentType EquipmentType { get; private set; }

        public int EquipmentStatusId { get; private set; }
        public EquipmentStatus EquipmentStatus { get; private set; }

        public int LocationId { get; private set; }
        public Location Location { get; private set; }

        private readonly List<EquipmentHistory> _historyEntries = new();
        public IReadOnlyCollection<EquipmentHistory> HistoryEntries => _historyEntries.AsReadOnly();

        private readonly List<InventoryRecord> _inventoryRecords = new();
        public IReadOnlyCollection<InventoryRecord> InventoryRecords => _inventoryRecords.AsReadOnly();

        private Equipment()
        {
            InventoryNumber = null!;
            Name = null!;
            EquipmentType = null!;
            EquipmentStatus = null!;
            Location = null!;
        }

        public Equipment(
            string inventoryNumber,
            string name,
            int equipmentTypeId,
            int equipmentStatusId,
            int locationId,
            string? serialNumber = null,
            string? model = null,
            string? manufacturer = null,
            DateTime? purchaseDate = null,
            DateTime? commissioningDate = null,
            DateTime? warrantyEndDate = null,
            string? responsiblePerson = null,
            string? notes = null)
        {
            SetInventoryNumber(inventoryNumber);
            SetName(name);
            SetDates(purchaseDate, commissioningDate, warrantyEndDate);

            EquipmentTypeId = ValidatePositiveId(equipmentTypeId, "Тип оборудования");
            EquipmentStatusId = ValidatePositiveId(equipmentStatusId, "Статус оборудования");
            LocationId = ValidatePositiveId(locationId, "Местоположение");

            SerialNumber = Normalize(serialNumber);
            Model = Normalize(model);
            Manufacturer = Normalize(manufacturer);
            ResponsiblePerson = Normalize(responsiblePerson);
            Notes = Normalize(notes);
        }

        public void UpdateCard(
            string inventoryNumber,
            string name,
            int equipmentTypeId,
            int equipmentStatusId,
            int locationId,
            string? serialNumber,
            string? model,
            string? manufacturer,
            DateTime? purchaseDate,
            DateTime? commissioningDate,
            DateTime? warrantyEndDate,
            string? responsiblePerson,
            string? notes)
        {
            SetInventoryNumber(inventoryNumber);
            SetName(name);
            SetDates(purchaseDate, commissioningDate, warrantyEndDate);

            EquipmentTypeId = ValidatePositiveId(equipmentTypeId, "Тип оборудования");
            EquipmentStatusId = ValidatePositiveId(equipmentStatusId, "Статус оборудования");
            LocationId = ValidatePositiveId(locationId, "Местоположение");

            SerialNumber = Normalize(serialNumber);
            Model = Normalize(model);
            Manufacturer = Normalize(manufacturer);
            ResponsiblePerson = Normalize(responsiblePerson);
            Notes = Normalize(notes);

            MarkAsUpdated();
        }

        public void ChangeStatus(int newStatusId)
        {
            EquipmentStatusId = ValidatePositiveId(newStatusId, "Статус оборудования");
            MarkAsUpdated();
        }

        public void ChangeLocation(int newLocationId)
        {
            LocationId = ValidatePositiveId(newLocationId, "Местоположение");
            MarkAsUpdated();
        }

        public void ChangeResponsiblePerson(string? responsiblePerson)
        {
            ResponsiblePerson = Normalize(responsiblePerson);
            MarkAsUpdated();
        }

        private void SetInventoryNumber(string inventoryNumber)
        {
            if (string.IsNullOrWhiteSpace(inventoryNumber))
                throw new DomainException("Инвентарный номер не может быть пустым.");

            InventoryNumber = inventoryNumber.Trim();
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Наименование оборудования не может быть пустым.");

            Name = name.Trim();
        }

        private void SetDates(DateTime? purchaseDate, DateTime? commissioningDate, DateTime? warrantyEndDate)
        {
            if (purchaseDate.HasValue && commissioningDate.HasValue &&
                commissioningDate.Value.Date < purchaseDate.Value.Date)
            {
                throw new DomainException("Дата ввода в эксплуатацию не может быть раньше даты покупки.");
            }

            if (commissioningDate.HasValue && warrantyEndDate.HasValue &&
                warrantyEndDate.Value.Date < commissioningDate.Value.Date)
            {
                throw new DomainException("Дата окончания гарантии не может быть раньше даты ввода в эксплуатацию.");
            }

            PurchaseDate = purchaseDate;
            CommissioningDate = commissioningDate;
            WarrantyEndDate = warrantyEndDate;
        }

        private static int ValidatePositiveId(int value, string fieldName)
        {
            if (value <= 0)
                throw new DomainException($"{fieldName} должен быть указан корректно.");

            return value;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
