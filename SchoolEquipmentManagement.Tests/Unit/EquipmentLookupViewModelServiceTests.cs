using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.Services.Equipment;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class EquipmentLookupViewModelServiceTests
    {
        [Fact]
        public async Task PopulateFormAsync_ShouldFillListsAndMarkSelectedValues()
        {
            var service = new EquipmentLookupViewModelService(new ConfigurableDictionaryService());
            var model = new EquipmentCreateViewModel
            {
                EquipmentTypeId = 2,
                EquipmentStatusId = 1,
                LocationId = 3
            };

            await service.PopulateFormAsync(model);

            Assert.Equal(2, model.EquipmentTypes.Count);
            Assert.Equal(3, model.Locations.Count);
            Assert.True(model.EquipmentTypes.Single(x => x.Value == "2").Selected);
            Assert.True(model.EquipmentStatuses.Single(x => x.Value == "1").Selected);
            Assert.True(model.Locations.Single(x => x.Value == "3").Selected);
        }

        [Fact]
        public async Task PopulateStatusOptionsAsync_ShouldExcludeWrittenOffStatus()
        {
            var service = new EquipmentLookupViewModelService(new ConfigurableDictionaryService());
            var model = new EquipmentChangeStatusViewModel
            {
                NewStatusId = 2
            };

            await service.PopulateStatusOptionsAsync(model);

            Assert.DoesNotContain(model.AvailableStatuses, x => x.Text == "Списано");
            Assert.True(model.AvailableStatuses.Single(x => x.Value == "2").Selected);
        }

        [Fact]
        public async Task GetWrittenOffStatusIdAsync_ShouldThrow_WhenDictionaryDoesNotContainRequiredStatus()
        {
            var service = new EquipmentLookupViewModelService(new ConfigurableDictionaryService(includeWrittenOff: false));

            var action = () => service.GetWrittenOffStatusIdAsync();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
            Assert.Contains("отсутствует статус", exception.Message);
        }

        [Fact]
        public async Task CreateHistoryValueResolverAsync_ShouldReturnLookupMaps()
        {
            var service = new EquipmentLookupViewModelService(new ConfigurableDictionaryService());

            var resolver = await service.CreateHistoryValueResolverAsync();

            Assert.Equal("Ноутбук", resolver["EquipmentTypeId"]["1"]);
            Assert.Equal("В эксплуатации", resolver["EquipmentStatusId"]["1"]);
            Assert.Equal("Кабинет 102", resolver["LocationId"]["2"]);
        }

        private sealed class ConfigurableDictionaryService : IDictionaryService
        {
            private readonly bool _includeWrittenOff;

            public ConfigurableDictionaryService(bool includeWrittenOff = true)
            {
                _includeWrittenOff = includeWrittenOff;
            }

            public Task<List<LookupItemDto>> GetEquipmentTypesAsync() =>
                Task.FromResult(new List<LookupItemDto>
                {
                    new() { Id = 1, Name = "Ноутбук" },
                    new() { Id = 2, Name = "Принтер" }
                });

            public Task<List<LookupItemDto>> GetEquipmentStatusesAsync()
            {
                var items = new List<LookupItemDto>
                {
                    new() { Id = 1, Name = "В эксплуатации" },
                    new() { Id = 2, Name = "На складе" }
                };

                if (_includeWrittenOff)
                {
                    items.Add(new LookupItemDto { Id = 5, Name = "Списано" });
                }

                return Task.FromResult(items);
            }

            public Task<List<LookupItemDto>> GetLocationsAsync() =>
                Task.FromResult(new List<LookupItemDto>
                {
                    new() { Id = 1, Name = "Кабинет 101" },
                    new() { Id = 2, Name = "Кабинет 102" },
                    new() { Id = 3, Name = "Склад" }
                });
        }
    }
}
