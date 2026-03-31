using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class EquipmentStatus : BaseEntity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }

        private readonly List<Equipment> _equipmentItems = new();
        public IReadOnlyCollection<Equipment> EquipmentItems => _equipmentItems.AsReadOnly();

        private EquipmentStatus()
        {
            Name = null!;
        }

        public EquipmentStatus(string name, string? description = null)
        {
            SetName(name);
            SetDescription(description);
        }

        public void Update(string name, string? description)
        {
            SetName(name);
            SetDescription(description);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Наименование статуса оборудования не может быть пустым.");

            Name = name.Trim();
        }

        private void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }
    }
}
