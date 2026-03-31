using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Tests.TestSupport;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.Services.Inventory;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class InventoryViewModelFactoryTests
    {
        [Fact]
        public async Task CreateIndexViewModelAsync_ShouldMapSessionsAndPermissions()
        {
            var inventoryService = new FakeInventoryService
            {
                Sessions = new List<InventorySessionListItemDto>
                {
                    new()
                    {
                        Id = 5,
                        Name = "\u0412\u0435\u0441\u0435\u043D\u043D\u044F\u044F \u0438\u043D\u0432\u0435\u043D\u0442\u0430\u0440\u0438\u0437\u0430\u0446\u0438\u044F",
                        StartDate = new DateTime(2026, 3, 30),
                        Status = "\u0410\u043A\u0442\u0438\u0432\u043D\u0430",
                        CreatedBy = "Tester",
                        CheckedCount = 8,
                        FoundCount = 7,
                        MissingCount = 1,
                        DiscrepancyCount = 2
                    }
                }
            };

            var factory = new InventoryViewModelFactory(
                inventoryService,
            new FakeUserAccessService(ModulePermission.CreateInventorySession));

            var model = await factory.CreateIndexViewModelAsync();

            Assert.True(model.CanCreateSession);
            var session = Assert.Single(model.Sessions);
            Assert.Equal(5, session.Id);
            Assert.Equal("bg-primary text-white", session.StatusBadgeClass);
        }

        [Fact]
        public async Task CreateDetailsViewModelAsync_ShouldMapItemsAndDerivedPresentation()
        {
            var inventoryService = new FakeInventoryService
            {
                SessionDetails = new InventorySessionDetailsDto
                {
                    Id = 3,
                    Name = "\u041C\u0430\u0440\u0442 2026",
                    StartDate = new DateTime(2026, 3, 30),
                    Status = "\u0410\u043A\u0442\u0438\u0432\u043D\u0430",
                    CreatedBy = "Auditor",
                    TotalEquipmentCount = 10,
                    CheckedCount = 4,
                    FoundCount = 3,
                    MissingCount = 1,
                    DiscrepancyCount = 1,
                    EquipmentItems = new List<InventorySessionEquipmentItemDto>
                    {
                        new()
                        {
                            EquipmentId = 17,
                            InventoryNumber = "INV-017",
                            Name = "\u041D\u043E\u0443\u0442\u0431\u0443\u043A",
                            EquipmentTypeName = "\u041D\u043E\u0443\u0442\u0431\u0443\u043A",
                            ExpectedLocationName = "\u041A\u0430\u0431\u0438\u043D\u0435\u0442 101",
                            EquipmentStatusName = "\u0412 \u044D\u043A\u0441\u043F\u043B\u0443\u0430\u0442\u0430\u0446\u0438\u0438",
                            IsChecked = true,
                            IsFound = true,
                            ActualLocationName = "\u041A\u0430\u0431\u0438\u043D\u0435\u0442 102",
                            ConditionComment = null,
                            HasLocationDiscrepancy = true
                        }
                    }
                }
            };

            var factory = new InventoryViewModelFactory(
                inventoryService,
            new FakeUserAccessService(ModulePermission.ManageInventorySession, ModulePermission.CheckInventory));

            var model = await factory.CreateDetailsViewModelAsync(3);

            Assert.NotNull(model);
            Assert.True(model!.CanManageSession);
            Assert.True(model.CanCheckInventory);
            Assert.True(model.CanComplete);
            Assert.Equal("bg-primary text-white", model.StatusBadgeClass);

            var item = Assert.Single(model.EquipmentItems);
            Assert.True(item.HasActualLocationNote);
            Assert.Equal("\u041D\u0430\u0439\u0434\u0435\u043D\u043E \u0441 \u0440\u0430\u0441\u0445\u043E\u0436\u0434\u0435\u043D\u0438\u0435\u043C", item.ResultText);
            Assert.Equal("bg-warning text-dark", item.ResultBadgeClass);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", item.DisplayConditionComment);
            Assert.Equal("\u0418\u0437\u043C\u0435\u043D\u0438\u0442\u044C", item.CheckActionText);
        }

        [Fact]
        public async Task CreateCheckViewModelAsync_ShouldMapCheckItemAndDefaultIsFoundToTrue()
        {
            var inventoryService = new FakeInventoryService
            {
                CheckItem = new InventorySessionEquipmentItemDto
                {
                    EquipmentId = 11,
                    InventoryNumber = "INV-011",
                    Name = "\u041F\u0440\u043E\u0435\u043A\u0442\u043E\u0440",
                    ExpectedLocationName = "\u0410\u043A\u0442\u043E\u0432\u044B\u0439 \u0437\u0430\u043B",
                    EquipmentStatusName = "\u0412 \u044D\u043A\u0441\u043F\u043B\u0443\u0430\u0442\u0430\u0446\u0438\u0438",
                    IsFound = null,
                    ActualLocationId = 4,
                    ConditionComment = "\u0422\u0440\u0435\u0431\u0443\u0435\u0442 \u043F\u0440\u043E\u0444\u0438\u043B\u0430\u043A\u0442\u0438\u043A\u0443"
                }
            };

            var factory = new InventoryViewModelFactory(
                inventoryService,
            new FakeUserAccessService());

            var model = await factory.CreateCheckViewModelAsync(9, 11);

            Assert.NotNull(model);
            Assert.Equal(9, model!.SessionId);
            Assert.Equal(11, model.EquipmentId);
            Assert.True(model.IsFound);
            Assert.Equal(4, model.ActualLocationId);
            Assert.Equal("\u0422\u0440\u0435\u0431\u0443\u0435\u0442 \u043F\u0440\u043E\u0444\u0438\u043B\u0430\u043A\u0442\u0438\u043A\u0443", model.ConditionComment);
        }

        [Fact]
        public async Task CreateDetailsViewModelAsync_ShouldReturnNull_WhenSessionNotFound()
        {
            var factory = new InventoryViewModelFactory(
                new FakeInventoryService(),
            new FakeUserAccessService());

            var model = await factory.CreateDetailsViewModelAsync(404);

            Assert.Null(model);
        }

        [Fact]
        public async Task CreateDetailsViewModelAsync_ShouldExposeReadOnlyStateAndUncheckedPresentation()
        {
            var inventoryService = new FakeInventoryService
            {
                SessionDetails = new InventorySessionDetailsDto
                {
                    Id = 12,
                    Name = "\u0417\u0430\u0432\u0435\u0440\u0448\u0435\u043D\u043D\u0430\u044F \u0441\u0435\u0441\u0441\u0438\u044F",
                    StartDate = new DateTime(2026, 3, 20),
                    EndDate = new DateTime(2026, 3, 25),
                    Status = "\u0417\u0430\u0432\u0435\u0440\u0448\u0435\u043D\u0430",
                    CreatedBy = "Auditor",
                    TotalEquipmentCount = 2,
                    CheckedCount = 2,
                    FoundCount = 1,
                    MissingCount = 1,
                    DiscrepancyCount = 0,
                    EquipmentItems = new List<InventorySessionEquipmentItemDto>
                    {
                        new()
                        {
                            EquipmentId = 21,
                            InventoryNumber = "INV-021",
                            Name = "\u0421\u043A\u0430\u043D\u0435\u0440",
                            EquipmentTypeName = "\u041F\u0435\u0440\u0438\u0444\u0435\u0440\u0438\u044F",
                            ExpectedLocationName = "\u0411\u0438\u0431\u043B\u0438\u043E\u0442\u0435\u043A\u0430",
                            EquipmentStatusName = "\u0412 \u044D\u043A\u0441\u043F\u043B\u0443\u0430\u0442\u0430\u0446\u0438\u0438",
                            IsChecked = false,
                            IsFound = null,
                            ActualLocationName = "\u0411\u0438\u0431\u043B\u0438\u043E\u0442\u0435\u043A\u0430",
                            ConditionComment = " "
                        }
                    }
                }
            };

            var factory = new InventoryViewModelFactory(
                inventoryService,
            new FakeUserAccessService(ModulePermission.CheckInventory));

            var model = await factory.CreateDetailsViewModelAsync(12);

            Assert.NotNull(model);
            Assert.True(model!.IsReadOnly);
            Assert.False(model.CanStart);
            Assert.False(model.CanComplete);
            Assert.Equal("bg-success text-white", model.StatusBadgeClass);
            Assert.False(model.CanManageSession);
            Assert.True(model.CanCheckInventory);

            var item = Assert.Single(model.EquipmentItems);
            Assert.Equal("\u041D\u0435 \u043F\u0440\u043E\u0432\u0435\u0440\u0435\u043D\u043E", item.ResultText);
            Assert.Equal("bg-secondary text-white", item.ResultBadgeClass);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", item.DisplayConditionComment);
            Assert.Equal("\u041F\u0440\u043E\u0432\u0435\u0440\u0438\u0442\u044C", item.CheckActionText);
            Assert.False(item.HasActualLocationNote);
        }

        [Fact]
        public async Task CreateCheckViewModelAsync_ShouldReturnNull_WhenItemNotFound()
        {
            var factory = new InventoryViewModelFactory(
                new FakeInventoryService(),
            new FakeUserAccessService());

            var model = await factory.CreateCheckViewModelAsync(3, 99);

            Assert.Null(model);
        }

        [Fact]
        public async Task CreateCheckViewModelAsync_ShouldPreserveFalseIsFoundValue()
        {
            var inventoryService = new FakeInventoryService
            {
                CheckItem = new InventorySessionEquipmentItemDto
                {
                    EquipmentId = 14,
                    InventoryNumber = "INV-014",
                    Name = "\u041C\u043E\u043D\u0438\u0442\u043E\u0440",
                    ExpectedLocationName = "\u041A\u0430\u0431\u0438\u043D\u0435\u0442 201",
                    EquipmentStatusName = "\u0412 \u044D\u043A\u0441\u043F\u043B\u0443\u0430\u0442\u0430\u0446\u0438\u0438",
                    IsFound = false,
                    ActualLocationId = null,
                    ConditionComment = null
                }
            };

            var factory = new InventoryViewModelFactory(
                inventoryService,
            new FakeUserAccessService());

            var model = await factory.CreateCheckViewModelAsync(4, 14);

            Assert.NotNull(model);
            Assert.False(model!.IsFound);
            Assert.Null(model.ActualLocationId);
        }
    }
}
