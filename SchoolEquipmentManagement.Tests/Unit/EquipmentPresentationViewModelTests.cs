using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class EquipmentPresentationViewModelTests
    {
        [Fact]
        public void EquipmentListViewModel_ShouldExposeBadgeAndResponsibleDisplay()
        {
            var model = new EquipmentListViewModel
            {
                Status = "\u0412 \u044D\u043A\u0441\u043F\u043B\u0443\u0430\u0442\u0430\u0446\u0438\u0438",
                ResponsiblePerson = null
            };

            Assert.Equal("bg-success text-white", model.StatusBadgeClass);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", model.DisplayResponsiblePerson);
        }

        [Fact]
        public void EquipmentIndexViewModel_ShouldCalculatePaginationPresentation()
        {
            var model = new EquipmentIndexViewModel
            {
                Page = 3,
                PageSize = 20,
                TotalCount = 77,
                TotalPages = 4
            };

            Assert.Equal(41, model.StartItem);
            Assert.Equal(60, model.EndItem);
            Assert.False(model.IsFirstPage);
            Assert.False(model.IsLastPage);
            Assert.Equal(2, model.PreviousPage);
            Assert.Equal(4, model.NextPage);
        }

        [Fact]
        public void EquipmentDetailsViewModel_ShouldExposeDisplayValuesAndHistoryPagination()
        {
            var model = new EquipmentDetailsViewModel
            {
                Manufacturer = null,
                Model = "Latitude 7420",
                SerialNumber = null,
                PurchaseDate = new DateTime(2024, 9, 1),
                CommissioningDate = null,
                WarrantyEndDate = new DateTime(2027, 9, 1),
                ResponsiblePerson = null,
                Notes = null,
                LastChangedAt = new DateTime(2026, 3, 30),
                LastChangedBy = null,
                HistoryPage = 2,
                HistoryPageSize = 10,
                HistoryTotalCount = 23,
                HistoryTotalPages = 3
            };

            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", model.DisplayManufacturer);
            Assert.Equal("Latitude 7420", model.DisplayModel);
            Assert.Equal("01.09.2024", model.DisplayPurchaseDate);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", model.DisplayCommissioningDate);
            Assert.Equal("01.09.2027", model.DisplayWarrantyEndDate);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", model.DisplayResponsiblePerson);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", model.DisplayNotes);
            Assert.Equal("30.03.2026", model.DisplayLastChangedAt);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", model.DisplayLastChangedBy);
            Assert.Equal(11, model.HistoryStartItem);
            Assert.Equal(20, model.HistoryEndItem);
            Assert.Equal(1, model.PreviousHistoryPage);
            Assert.Equal(3, model.NextHistoryPage);
        }

        [Fact]
        public void EquipmentMovementViewModel_ShouldExposeDisplayFields()
        {
            var model = new EquipmentMovementViewModel
            {
                OccurredAt = new DateTime(2026, 3, 30, 12, 45, 0, DateTimeKind.Utc),
                Details = null,
                ChangedBy = null
            };

            Assert.False(model.HasDetails);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", model.DisplayDetails);
            Assert.Equal("\u041D\u0435 \u0443\u043A\u0430\u0437\u0430\u043D\u043E", model.DisplayChangedBy);
            Assert.False(string.IsNullOrWhiteSpace(model.DisplayOccurredAt));
        }
    }
}
