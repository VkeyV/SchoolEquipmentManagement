using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Application.Services;
using SchoolEquipmentManagement.Domain.Entities;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class DictionaryServiceTests
    {
        [Fact]
        public async Task GetEquipmentTypesAsync_ShouldMapRepositoryItems()
        {
            var service = new DictionaryService(new StubDictionaryRepository
            {
                EquipmentTypes =
                [
                    new EquipmentType("Ноутбук", "Мобильное устройство"),
                    new EquipmentType("Принтер")
                ]
            });

            var result = await service.GetEquipmentTypesAsync();

            Assert.Collection(
                result,
                item =>
                {
                    Assert.Equal("Ноутбук", item.Name);
                },
                item =>
                {
                    Assert.Equal("Принтер", item.Name);
                });
        }

        [Fact]
        public async Task GetEquipmentStatusesAsync_ShouldMapRepositoryItems()
        {
            var service = new DictionaryService(new StubDictionaryRepository
            {
                EquipmentStatuses =
                [
                    new EquipmentStatus("В эксплуатации"),
                    new EquipmentStatus("В ремонте")
                ]
            });

            var result = await service.GetEquipmentStatusesAsync();

            Assert.Collection(
                result,
                item => Assert.Equal("В эксплуатации", item.Name),
                item => Assert.Equal("В ремонте", item.Name));
        }

        [Fact]
        public async Task GetLocationsAsync_ShouldReturnDisplayNames()
        {
            var service = new DictionaryService(new StubDictionaryRepository
            {
                Locations =
                [
                    new Location("Главный корпус", "Кабинет 101"),
                    new Location("Лабораторный корпус", "Серверная")
                ]
            });

            var result = await service.GetLocationsAsync();

            Assert.Collection(
                result,
                item => Assert.Equal("Главный корпус, Кабинет 101", item.Name),
                item => Assert.Equal("Лабораторный корпус, Серверная", item.Name));
        }

        private sealed class StubDictionaryRepository : IDictionaryRepository
        {
            public List<EquipmentType> EquipmentTypes { get; init; } = [];
            public List<EquipmentStatus> EquipmentStatuses { get; init; } = [];
            public List<Location> Locations { get; init; } = [];

            public Task<List<EquipmentType>> GetEquipmentTypesAsync() => Task.FromResult(EquipmentTypes);

            public Task<List<EquipmentStatus>> GetEquipmentStatusesAsync() => Task.FromResult(EquipmentStatuses);

            public Task<List<Location>> GetLocationsAsync() => Task.FromResult(Locations);
        }
    }
}
