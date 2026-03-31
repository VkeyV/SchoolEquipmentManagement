using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public interface IEquipmentDetailsViewModelFactory
{
    Task<EquipmentDetailsViewModel?> CreateAsync(int id, int historyPage, bool includeFullHistory, Func<int, string> detailsUrlFactory);
}
