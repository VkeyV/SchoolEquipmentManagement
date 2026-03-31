using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class ReferenceEntitiesDomainTests
    {
        [Fact]
        public void EquipmentType_ShouldTrimValues_AndAllowUpdate()
        {
            var entity = new EquipmentType("  Ноутбук  ", "  Мобильное устройство  ");

            entity.Update("  Планшет  ", "   ");

            Assert.Equal("Планшет", entity.Name);
            Assert.Null(entity.Description);
        }

        [Fact]
        public void EquipmentStatus_ShouldThrow_WhenNameIsEmpty()
        {
            var action = () => new EquipmentStatus("   ");

            var exception = Assert.Throws<DomainException>(action);
            Assert.Contains("статуса оборудования", exception.Message);
        }

        [Fact]
        public void Location_ShouldBuildDisplayName_AndTrimFields()
        {
            var location = new Location("  Главный корпус  ", "  Кабинет 305  ", "  Учебный класс  ");

            Assert.Equal("Главный корпус", location.Building);
            Assert.Equal("Кабинет 305", location.Room);
            Assert.Equal("Учебный класс", location.Description);
            Assert.Equal("Главный корпус, Кабинет 305", location.GetDisplayName());
        }

        [Fact]
        public void Location_Update_ShouldThrow_WhenRoomIsEmpty()
        {
            var location = new Location("Главный корпус", "Кабинет 101");

            var action = () => location.Update("Главный корпус", " ", null);

            var exception = Assert.Throws<DomainException>(action);
            Assert.Contains("Кабинет или помещение", exception.Message);
        }
    }
}
