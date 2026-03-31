using SchoolEquipmentManagement.Domain.Common;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Domain.Entities
{
    public class Location : BaseEntity
    {
        public string Building { get; private set; }
        public string Room { get; private set; }
        public string? Description { get; private set; }

        private readonly List<Equipment> _equipmentItems = new();
        public IReadOnlyCollection<Equipment> EquipmentItems => _equipmentItems.AsReadOnly();

        private Location()
        {
            Building = null!;
            Room = null!;
        }

        public Location(string building, string room, string? description = null)
        {
            SetBuilding(building);
            SetRoom(room);
            SetDescription(description);
        }

        public void Update(string building, string room, string? description)
        {
            SetBuilding(building);
            SetRoom(room);
            SetDescription(description);
        }

        public string GetDisplayName()
        {
            return $"{Building}, {Room}";
        }

        private void SetBuilding(string building)
        {
            if (string.IsNullOrWhiteSpace(building))
                throw new DomainException("Корпус или здание не может быть пустым.");

            Building = building.Trim();
        }

        private void SetRoom(string room)
        {
            if (string.IsNullOrWhiteSpace(room))
                throw new DomainException("Кабинет или помещение не может быть пустым.");

            Room = room.Trim();
        }

        private void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }
    }
}
