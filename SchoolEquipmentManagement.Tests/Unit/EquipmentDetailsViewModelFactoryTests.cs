using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Tests.TestSupport;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.Services.Equipment;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class EquipmentDetailsViewModelFactoryTests
    {
        [Fact]
        public async Task CreateAsync_ShouldReturnNull_WhenEquipmentNotFound()
        {
            var factory = new EquipmentDetailsViewModelFactory(
                new FakeEquipmentService(),
                new ResolverLookupService(),
            new FakeUserAccessService(),
                new DeterministicEquipmentMediaService());

            var model = await factory.CreateAsync(15, 1, false, id => $"/equipment/{id}");

            Assert.Null(model);
        }

        [Fact]
        public async Task CreateAsync_ShouldMapPresentationHistoryAndPermissions()
        {
            var equipmentService = new FakeEquipmentService
            {
                DetailsResult = new EquipmentDetailsDto
                {
                    Id = 7,
                    InventoryNumber = "INV-007",
                    Name = "Рабочая станция преподавателя",
                    EquipmentTypeName = "Ноутбук",
                    EquipmentStatusName = "В эксплуатации",
                    LocationName = "Главный корпус, Кабинет 101",
                    SerialNumber = "SN-007",
                    Manufacturer = "Dell",
                    Model = "OptiPlex 7090",
                    PurchaseDate = new DateTime(2025, 9, 1),
                    CommissioningDate = new DateTime(2025, 9, 10),
                    WarrantyEndDate = DateTime.Today.AddDays(12),
                    ResponsiblePerson = "Иванов И.И.",
                    Notes = "Закреплено за кафедрой",
                    History = new List<EquipmentHistoryItemDto>
                    {
                        new()
                        {
                            Id = 301,
                            ActionType = "InventoryChecked",
                            ChangedField = "LocationId",
                            NewValue = "2",
                            Comment = "Подтверждено при проверке",
                            ChangedBy = "Auditor",
                            ChangedAt = new DateTime(2026, 3, 30)
                        },
                        new()
                        {
                            Id = 300,
                            ActionType = "StatusChanged",
                            ChangedField = "EquipmentStatusId",
                            OldValue = "3",
                            NewValue = "1",
                            Comment = "Возвращено в работу",
                            ChangedBy = "Admin",
                            ChangedAt = new DateTime(2026, 3, 29)
                        },
                        new()
                        {
                            Id = 299,
                            ActionType = "Created",
                            Comment = "Первичная постановка на учет",
                            ChangedBy = "Admin",
                            ChangedAt = new DateTime(2025, 9, 10)
                        }
                    }
                }
            };

            var factory = new EquipmentDetailsViewModelFactory(
                equipmentService,
                new ResolverLookupService(),
            new FakeUserAccessService(
                    ModulePermission.EditEquipment,
                    ModulePermission.ChangeEquipmentStatus,
                    ModulePermission.ChangeEquipmentLocation,
                    ModulePermission.WriteOffEquipment),
                new DeterministicEquipmentMediaService());

            var model = await factory.CreateAsync(7, 5, false, id => $"https://example.test/equipment/{id}");

            Assert.NotNull(model);
            Assert.Equal(7, model!.Id);
            Assert.Equal("photo://7", model.PhotoSource);
            Assert.Equal("qr://https://example.test/equipment/7", model.QrCodeSource);
            Assert.Equal("code://INV-007", model.CodeDataUri);
            Assert.Equal("Dell, OptiPlex 7090, серийный номер SN-007.", model.ServiceSummary);
            Assert.Equal("Эксплуатация", model.LifecycleStage);
            Assert.Equal("Гарантия скоро истекает", model.WarrantyStatus);
            Assert.Equal("Закреплено за Иванов И.И.. Текущее размещение: Главный корпус, Кабинет 101.", model.OwnershipSummary);
            Assert.Equal("30.03.2026: Подтверждено при проверке", model.LastInventorySummary);
            Assert.True(model.CanEdit);
            Assert.True(model.CanChangeStatus);
            Assert.True(model.CanChangeLocation);
            Assert.True(model.CanWriteOff);
            Assert.Equal(1, model.HistoryPage);
            Assert.Equal(1, model.HistoryTotalPages);
            Assert.Equal(3, model.HistoryTotalCount);

            var statusHistory = Assert.Single(model.History.Where(x => x.ActionType == "Смена статуса"));
            Assert.Equal("Статус", statusHistory.ChangedField);
            Assert.Equal("На складе", statusHistory.OldValue);
            Assert.Equal("В эксплуатации", statusHistory.NewValue);
            Assert.Equal("bg-warning text-dark", statusHistory.BadgeClass);

            var inventoryMovement = Assert.Single(model.Movements.Where(x => x.EventName == "Инвентаризация"));
            Assert.Equal("Подтверждено при проверке", inventoryMovement.Summary);
            Assert.Equal("Кабинет 102", inventoryMovement.Details);
        }

        [Fact]
        public async Task CreateAsync_ShouldUseFullHistoryMode_WhenRequested()
        {
            var equipmentService = new FakeEquipmentService
            {
                DetailsResult = new EquipmentDetailsDto
                {
                    Id = 9,
                    InventoryNumber = "INV-009",
                    Name = "Принтер",
                    EquipmentTypeName = "Периферия",
                    EquipmentStatusName = "На складе",
                    LocationName = "Склад",
                    History = new List<EquipmentHistoryItemDto>
                    {
                        new() { Id = 1, ActionType = "Created", ChangedBy = "Admin", ChangedAt = new DateTime(2026, 1, 1) },
                        new() { Id = 2, ActionType = "Updated", ChangedBy = "Admin", ChangedAt = new DateTime(2026, 1, 2) },
                        new() { Id = 3, ActionType = "LocationChanged", ChangedField = "LocationId", OldValue = "1", NewValue = "2", ChangedBy = "Admin", ChangedAt = new DateTime(2026, 1, 3) }
                    }
                }
            };

            var factory = new EquipmentDetailsViewModelFactory(
                equipmentService,
                new ResolverLookupService(),
            new FakeUserAccessService(),
                new DeterministicEquipmentMediaService());

            var model = await factory.CreateAsync(9, 1, true, id => $"/equipment/{id}");

            Assert.NotNull(model);
            Assert.Equal(3, model!.HistoryPageSize);
            Assert.Equal(3, model.History.Count);
            Assert.Equal(2, model.Movements.Count);
        }

        private sealed class ResolverLookupService : IEquipmentLookupViewModelService
        {
            public Task PopulateFormAsync(EquipmentCreateViewModel model) => Task.CompletedTask;

            public Task PopulateIndexAsync(EquipmentIndexViewModel model) => Task.CompletedTask;

            public Task PopulateStatusOptionsAsync(EquipmentChangeStatusViewModel model) => Task.CompletedTask;

            public Task PopulateLocationOptionsAsync(EquipmentChangeLocationViewModel model) => Task.CompletedTask;

            public Task<int> GetWrittenOffStatusIdAsync() => Task.FromResult(5);

            public Task<Dictionary<string, Dictionary<string, string>>> CreateHistoryValueResolverAsync()
            {
                return Task.FromResult(new Dictionary<string, Dictionary<string, string>>
                {
                    ["EquipmentStatusId"] = new()
                    {
                        ["1"] = "В эксплуатации",
                        ["3"] = "На складе"
                    },
                    ["LocationId"] = new()
                    {
                        ["1"] = "Кабинет 101",
                        ["2"] = "Кабинет 102"
                    },
                    ["EquipmentTypeId"] = new()
                    {
                        ["1"] = "Ноутбук"
                    }
                });
            }
        }

        private sealed class DeterministicEquipmentMediaService : IEquipmentMediaService
        {
            public string ResolvePhotoSource(int equipmentId, string name, string equipmentType, string inventoryNumber, bool preferUploadedFileOnly = false) =>
                $"photo://{equipmentId}";

            public byte[]? GetPhotoBytes(int equipmentId) => null;

            public Task SavePhotoAsync(int equipmentId, Microsoft.AspNetCore.Http.IFormFile photo) => Task.CompletedTask;

            public void RemovePhoto(int equipmentId)
            {
            }

            public string BuildQrCodeSource(string detailsUrl) => $"qr://{detailsUrl}";

            public byte[] BuildQrCodeBytes(string detailsUrl) => Array.Empty<byte>();

            public string BuildCodeDataUri(string inventoryNumber) => $"code://{inventoryNumber}";

            public string SanitizeFileName(string value) => value;
        }
    }
}
