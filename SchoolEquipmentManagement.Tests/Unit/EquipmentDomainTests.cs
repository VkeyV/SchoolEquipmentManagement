using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Domain.Exceptions;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class EquipmentDomainTests
    {
        [Fact]
        public void Constructor_ShouldThrow_WhenCommissioningDateEarlierThanPurchaseDate()
        {
            var purchaseDate = new DateTime(2026, 3, 10);
            var commissioningDate = new DateTime(2026, 3, 9);

            var action = () => new Equipment("INV-001", "Ноутбук", 1, 1, 1, purchaseDate: purchaseDate, commissioningDate: commissioningDate);

            var exception = Assert.Throws<DomainException>(action);
            Assert.Contains("ввода в эксплуатацию", exception.Message);
        }

        [Fact]
        public void UpdateCard_ShouldThrow_WhenWarrantyEarlierThanCommissioningDate()
        {
            var equipment = new Equipment("INV-001", "Ноутбук", 1, 1, 1);

            var action = () => equipment.UpdateCard(
                "INV-001",
                "Ноутбук",
                1,
                1,
                1,
                null,
                null,
                null,
                new DateTime(2026, 3, 1),
                new DateTime(2026, 3, 5),
                new DateTime(2026, 3, 4),
                null,
                null);

            var exception = Assert.Throws<DomainException>(action);
            Assert.Contains("окончания гарантии", exception.Message);
        }
    }
}
