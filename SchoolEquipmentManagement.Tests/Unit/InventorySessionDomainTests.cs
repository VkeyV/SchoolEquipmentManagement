using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class InventorySessionDomainTests
    {
        [Fact]
        public void Start_ShouldMoveSessionToInProgress()
        {
            var session = new InventorySession("Плановая инвентаризация", new DateTime(2026, 3, 29), "Tester");

            session.Start();

            Assert.Equal(InventorySessionStatus.InProgress, session.Status);
        }

        [Fact]
        public void Complete_ShouldThrow_WhenSessionIsNotInProgress()
        {
            var session = new InventorySession("Плановая инвентаризация", new DateTime(2026, 3, 29), "Tester");

            var action = () => session.Complete(new DateTime(2026, 3, 30));

            var exception = Assert.Throws<DomainException>(action);
            Assert.Contains("Завершить можно только", exception.Message);
        }
    }
}
