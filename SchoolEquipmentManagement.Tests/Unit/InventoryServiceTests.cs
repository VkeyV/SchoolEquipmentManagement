using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Services;
using SchoolEquipmentManagement.Domain.Exceptions;
using SchoolEquipmentManagement.Tests.TestSupport;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class InventoryServiceTests
    {
        [Fact]
        public async Task RecordCheckAsync_ShouldThrow_WhenSessionIsNotActive()
        {
            var sessionRepository = new FakeInventorySessionRepository();
            sessionRepository.Seed(TestEntityFactory.CreateSession(id: 1));

            var recordRepository = new FakeInventoryRecordRepository();
            var equipmentRepository = new FakeEquipmentRepository();
            equipmentRepository.Seed(TestEntityFactory.CreateEquipment(id: 1));

            var historyService = new FakeEquipmentHistoryService();
            var service = new InventoryService(sessionRepository, recordRepository, equipmentRepository, historyService);

            var action = () => service.RecordCheckAsync(new InventoryCheckDto
            {
                SessionId = 1,
                EquipmentId = 1,
                IsFound = true,
                CheckedBy = "Tester"
            });

            var exception = await Assert.ThrowsAsync<DomainException>(action);
            Assert.Contains("только для активной", exception.Message);
        }

        [Fact]
        public async Task RecordCheckAsync_ShouldCreateRecordAndWriteHistory_WhenCheckIsNew()
        {
            var session = TestEntityFactory.CreateSession(id: 1);
            session.Start();

            var sessionRepository = new FakeInventorySessionRepository();
            sessionRepository.Seed(session);

            var recordRepository = new FakeInventoryRecordRepository();
            var equipmentRepository = new FakeEquipmentRepository();
            equipmentRepository.Seed(TestEntityFactory.CreateEquipment(id: 7, locationId: 1));

            var historyService = new FakeEquipmentHistoryService();
            var service = new InventoryService(sessionRepository, recordRepository, equipmentRepository, historyService);

            await service.RecordCheckAsync(new InventoryCheckDto
            {
                SessionId = 1,
                EquipmentId = 7,
                IsFound = true,
                ActualLocationId = 1,
                ConditionComment = "Проверено без замечаний",
                CheckedBy = "Tester"
            });

            Assert.Single(recordRepository.Items);
            Assert.Single(historyService.Records);
            Assert.Equal("InventoryCheck", historyService.Records[0].ChangedField);
        }
    }
}
