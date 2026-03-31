using SchoolEquipmentManagement.Application.DTOs;

namespace SchoolEquipmentManagement.Application.Interfaces
{
    public interface IEquipmentImportService
    {
        Task<EquipmentImportPreviewDto> PreviewCsvAsync(Stream stream);
        Task<EquipmentImportResultDto> ImportAsync(IEnumerable<EquipmentImportApplyItemDto> items, string changedBy);
    }
}
