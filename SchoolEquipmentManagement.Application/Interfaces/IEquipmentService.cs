using SchoolEquipmentManagement.Application.DTOs;

namespace SchoolEquipmentManagement.Application.Interfaces
{
    public interface IEquipmentService
    {
        Task<PagedResultDto<EquipmentListItemDto>> GetEquipmentListAsync(EquipmentFilterDto filter);
        Task<LocationDetailsDto?> GetLocationDetailsAsync(int locationId);
        Task<IReadOnlyList<EquipmentWarrantyItemDto>> GetWarrantyReportAsync(EquipmentWarrantyFilterDto filter);
        Task<IReadOnlyList<EquipmentIssueItemDto>> GetProblemEquipmentAsync();
        Task<EquipmentDetailsDto?> GetEquipmentDetailsAsync(int id);
        Task<int> CreateEquipmentAsync(CreateEquipmentDto dto);
        Task UpdateEquipmentAsync(UpdateEquipmentDto dto);
        Task ChangeStatusAsync(ChangeEquipmentStatusDto dto);
        Task ChangeLocationAsync(ChangeEquipmentLocationDto dto);
        Task AssignResponsibleAsync(AssignEquipmentResponsibleDto dto);
        Task WriteOffAsync(WriteOffEquipmentDto dto);
    }
}
