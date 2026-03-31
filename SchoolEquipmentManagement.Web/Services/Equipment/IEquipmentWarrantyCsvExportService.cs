using SchoolEquipmentManagement.Application.DTOs;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public interface IEquipmentWarrantyCsvExportService
{
    byte[] Export(IReadOnlyList<EquipmentDetailsDto> items, Func<int, string> detailsUrlFactory);
}
