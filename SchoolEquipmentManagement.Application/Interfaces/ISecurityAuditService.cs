using SchoolEquipmentManagement.Application.DTOs;

namespace SchoolEquipmentManagement.Application.Interfaces
{
    public interface ISecurityAuditService
    {
        Task WriteAsync(SecurityAuditWriteDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SecurityAuditListItemDto>> GetRecentAsync(SecurityAuditFilterDto filter, CancellationToken cancellationToken = default);
    }
}
