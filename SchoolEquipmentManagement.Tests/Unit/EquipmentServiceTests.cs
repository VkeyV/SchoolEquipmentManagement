using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Services;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;
using SchoolEquipmentManagement.Tests.TestSupport;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class EquipmentServiceTests
    {
        [Fact]
        public async Task CreateEquipmentAsync_ShouldPersistEquipmentAndWriteCreatedHistory()
        {
            var repository = new FakeEquipmentRepository();
            var inventoryRecordRepository = new FakeInventoryRecordRepository();
            var historyService = new FakeEquipmentHistoryService();
            var service = new EquipmentService(repository, inventoryRecordRepository, historyService);

            var createdId = await service.CreateEquipmentAsync(new CreateEquipmentDto
            {
                InventoryNumber = "INV-100",
                Name = "Тестовый ноутбук",
                EquipmentTypeId = 1,
                EquipmentStatusId = 1,
                LocationId = 1,
                ChangedBy = "Tester"
            });

            Assert.True(createdId > 0);
            Assert.Single(await repository.GetAllAsync());
            Assert.Single(historyService.Records);
            Assert.Equal(HistoryActionType.Created, historyService.Records[0].ActionType);
        }

        [Fact]
        public async Task ChangeStatusAsync_ShouldThrow_WhenEquipmentAlreadyWrittenOff()
        {
            var repository = new FakeEquipmentRepository();
            repository.Seed(TestEntityFactory.CreateEquipment(id: 1, statusId: 5, statusName: "Списано"));

            var inventoryRecordRepository = new FakeInventoryRecordRepository();
            var historyService = new FakeEquipmentHistoryService();
            var service = new EquipmentService(repository, inventoryRecordRepository, historyService);

            var action = () => service.ChangeStatusAsync(new ChangeEquipmentStatusDto
            {
                EquipmentId = 1,
                NewStatusId = 2,
                ChangedBy = "Tester"
            });

            var exception = await Assert.ThrowsAsync<DomainException>(action);
            Assert.Contains("списанного оборудования", exception.Message);
            Assert.Empty(historyService.Records);
        }

        [Fact]
        public async Task GetProblemEquipmentAsync_ShouldReturnIssuesFromStatusesAndInventoryResults()
        {
            var repository = new FakeEquipmentRepository();
            repository.Seed(
            TestEntityFactory.CreateEquipment(id: 1, inventoryNumber: "INV-001", statusId: 2, statusName: "В ремонте"),
                TestEntityFactory.CreateEquipment(id: 2, inventoryNumber: "INV-002", statusId: 6, statusName: "Требует диагностики"),
                TestEntityFactory.CreateEquipment(id: 3, inventoryNumber: "INV-003", statusId: 1, statusName: "В эксплуатации"),
                TestEntityFactory.CreateEquipment(id: 4, inventoryNumber: "INV-004", statusId: 1, statusName: "В эксплуатации"));

            var inventoryRecordRepository = new FakeInventoryRecordRepository();
            var missingRecord = TestEntityFactory.CreateRecord(sessionId: 10, equipmentId: 3, isFound: false);
            var discrepancyRecord = TestEntityFactory.CreateRecord(sessionId: 10, equipmentId: 4, isFound: true, actualLocationId: 2);

            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.Id))!.SetValue(missingRecord, 1);
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.Id))!.SetValue(discrepancyRecord, 2);
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.CheckedAt))!.SetValue(missingRecord, new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc));
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.CheckedAt))!.SetValue(discrepancyRecord, new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc));
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.ActualLocation))!.SetValue(discrepancyRecord, new Location("Главный корпус", "Кабинет 102"));

            inventoryRecordRepository.Items.Add(missingRecord);
            inventoryRecordRepository.Items.Add(discrepancyRecord);

            var service = new EquipmentService(repository, inventoryRecordRepository, new FakeEquipmentHistoryService());

            var result = await service.GetProblemEquipmentAsync();

            Assert.Equal(4, result.Count);
            Assert.Contains(result, issue => issue.IssueCode == "repair" && issue.PriorityLabel == "Средний");
            Assert.Contains(result, issue => issue.IssueCode == "diagnostics" && issue.PriorityLabel == "Высокий");
            Assert.Contains(result, issue => issue.IssueCode == "missing" && issue.PriorityLabel == "Критичный");
            Assert.Contains(result, issue => issue.IssueCode == "discrepancy" && issue.ActualLocationName == "Главный корпус, Кабинет 102");
        }

        [Fact]
        public async Task GetLocationDetailsAsync_ShouldReturnEquipmentStatusSummaryAndDiscrepancies()
        {
            var repository = new FakeEquipmentRepository();
            repository.Seed(
                TestEntityFactory.CreateEquipment(id: 1, inventoryNumber: "INV-001", statusId: 1, statusName: "В эксплуатации"),
            TestEntityFactory.CreateEquipment(id: 2, inventoryNumber: "INV-002", statusId: 2, statusName: "В ремонте"),
                TestEntityFactory.CreateEquipment(id: 3, inventoryNumber: "INV-003", statusId: 1, statusName: "В эксплуатации"),
                TestEntityFactory.CreateEquipment(id: 4, inventoryNumber: "INV-004", locationId: 2, statusId: 1, statusName: "В эксплуатации"));

            var inventoryRecordRepository = new FakeInventoryRecordRepository();
            var missingRecord = TestEntityFactory.CreateRecord(sessionId: 10, equipmentId: 2, isFound: false);
            var movedOutRecord = TestEntityFactory.CreateRecord(sessionId: 10, equipmentId: 3, isFound: true, actualLocationId: 2);
            var movedInRecord = TestEntityFactory.CreateRecord(sessionId: 10, equipmentId: 4, isFound: true, actualLocationId: 1);

            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.Id))!.SetValue(missingRecord, 1);
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.Id))!.SetValue(movedOutRecord, 2);
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.Id))!.SetValue(movedInRecord, 3);
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.CheckedAt))!.SetValue(missingRecord, new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc));
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.CheckedAt))!.SetValue(movedOutRecord, new DateTime(2026, 3, 30, 9, 0, 0, DateTimeKind.Utc));
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.CheckedAt))!.SetValue(movedInRecord, new DateTime(2026, 3, 30, 10, 0, 0, DateTimeKind.Utc));
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.ActualLocation))!.SetValue(movedOutRecord, new Location("Лабораторный корпус", "Кабинет 305"));
            typeof(InventoryRecord).GetProperty(nameof(InventoryRecord.ActualLocation))!.SetValue(movedInRecord, new Location("Главный корпус", "Кабинет 101"));

            inventoryRecordRepository.Items.Add(missingRecord);
            inventoryRecordRepository.Items.Add(movedOutRecord);
            inventoryRecordRepository.Items.Add(movedInRecord);

            var service = new EquipmentService(repository, inventoryRecordRepository, new FakeEquipmentHistoryService());

            var result = await service.GetLocationDetailsAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Главный корпус, Кабинет 101", result!.Name);
            Assert.Equal(3, result.EquipmentCount);
            Assert.Equal(3, result.DiscrepancyCount);
            Assert.Equal(1, result.MissingCount);
            Assert.Equal(2, result.StatusSummary.Count);
            Assert.Contains(result.StatusSummary, item => item.StatusName == "В эксплуатации" && item.Count == 2);
        Assert.Contains(result.StatusSummary, item => item.StatusName == "В ремонте" && item.Count == 1);
            Assert.Contains(result.InventoryDiscrepancies, item => item.DiscrepancyCode == "missing" && item.InventoryNumber == "INV-002");
            Assert.Contains(result.InventoryDiscrepancies, item => item.DiscrepancyCode == "moved-out" && item.InventoryNumber == "INV-003");
            Assert.Contains(result.InventoryDiscrepancies, item => item.DiscrepancyCode == "moved-in" && item.InventoryNumber == "INV-004");
        }

        [Fact]
        public async Task GetEquipmentListAsync_ShouldApplyWarrantyFilter()
        {
            var repository = new FakeEquipmentRepository();
            repository.Seed(
                CreateEquipmentWithWarranty(id: 1, inventoryNumber: "INV-001", warrantyEndDate: DateTime.Today.AddDays(15)),
                CreateEquipmentWithWarranty(id: 2, inventoryNumber: "INV-002", warrantyEndDate: DateTime.Today.AddDays(75)),
                CreateEquipmentWithWarranty(id: 3, inventoryNumber: "INV-003", warrantyEndDate: DateTime.Today.AddDays(-5)));

            var service = new EquipmentService(repository, new FakeInventoryRecordRepository(), new FakeEquipmentHistoryService());

            var result = await service.GetEquipmentListAsync(new EquipmentFilterDto
            {
                WarrantyFilter = "30",
                Page = 1,
                PageSize = 10
            });

            var item = Assert.Single(result.Items);
            Assert.Equal("INV-001", item.InventoryNumber);
            Assert.Equal(15, item.WarrantyDaysLeft);
        }

        [Fact]
        public async Task GetWarrantyReportAsync_ShouldReturnExpiredItems_WhenExpiredFilterSelected()
        {
            var repository = new FakeEquipmentRepository();
            repository.Seed(
                CreateEquipmentWithWarranty(id: 1, inventoryNumber: "INV-001", warrantyEndDate: DateTime.Today.AddDays(-1)),
                CreateEquipmentWithWarranty(id: 2, inventoryNumber: "INV-002", warrantyEndDate: DateTime.Today.AddDays(20)));

            var service = new EquipmentService(repository, new FakeInventoryRecordRepository(), new FakeEquipmentHistoryService());

            var result = await service.GetWarrantyReportAsync(new EquipmentWarrantyFilterDto
            {
                WarrantyFilter = "expired"
            });

            var item = Assert.Single(result);
            Assert.Equal("INV-001", item.InventoryNumber);
            Assert.True(item.WarrantyDaysLeft < 0);
        }

        [Fact]
        public async Task AssignResponsibleAsync_ShouldUpdateResponsiblePersonAndWriteHistory()
        {
            var repository = new FakeEquipmentRepository();
            repository.Seed(TestEntityFactory.CreateEquipment(id: 5, inventoryNumber: "INV-005"));

            var historyService = new FakeEquipmentHistoryService();
            var service = new EquipmentService(repository, new FakeInventoryRecordRepository(), historyService);

            await service.AssignResponsibleAsync(new AssignEquipmentResponsibleDto
            {
                EquipmentId = 5,
                ResponsiblePerson = "Сидоров С.С.",
                Comment = "Назначен новый кабинетный ответственный.",
                ChangedBy = "Tester"
            });

            var equipment = Assert.Single(await repository.GetAllAsync());
            Assert.Equal("Сидоров С.С.", equipment.ResponsiblePerson);

            var history = Assert.Single(historyService.Records);
            Assert.Equal(HistoryActionType.Updated, history.ActionType);
            Assert.Equal("ResponsiblePerson", history.ChangedField);
            Assert.Equal("Сидоров С.С.", history.NewValue);
        }

        private static Equipment CreateEquipmentWithWarranty(int id, string inventoryNumber, DateTime warrantyEndDate)
        {
            var equipment = TestEntityFactory.CreateEquipment(id: id, inventoryNumber: inventoryNumber);
            typeof(Equipment).GetProperty(nameof(Equipment.WarrantyEndDate))!.SetValue(equipment, warrantyEndDate);
            return equipment;
        }
    }
}
