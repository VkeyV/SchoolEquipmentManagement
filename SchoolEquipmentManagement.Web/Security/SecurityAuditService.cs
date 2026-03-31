using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Domain.Entities;
using SchoolEquipmentManagement.Infrastructure.Data;

namespace SchoolEquipmentManagement.Web.Security
{
    public sealed class SecurityAuditService : ISecurityAuditService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SecurityOptions _securityOptions;

        public SecurityAuditService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            IOptions<SecurityOptions> securityOptions)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _securityOptions = securityOptions.Value;
        }

        public async Task WriteAsync(SecurityAuditWriteDto dto, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dto);

            var httpContext = _httpContextAccessor.HttpContext;
            var entry = new SecurityAuditEntry(
                dto.EventType,
                dto.IsSuccessful,
                TrimToLength(dto.Summary, 256) ?? "Событие безопасности",
                TrimToLength(dto.UserName, 64),
                TrimToLength(dto.TargetUserName, 64),
                TrimToLength(httpContext?.Connection.RemoteIpAddress?.ToString(), 64),
                TrimToLength(httpContext?.Request.Headers["User-Agent"].ToString(), 512));

            await _dbContext.SecurityAuditEntries.AddAsync(entry, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SecurityAuditListItemDto>> GetRecentAsync(SecurityAuditFilterDto filter, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var take = Math.Clamp(filter.Take > 0 ? filter.Take : _securityOptions.AuditPageSize, 20, 500);
            var query = _dbContext.SecurityAuditEntries.AsNoTracking();

            if (filter.FailuresOnly)
            {
                query = query.Where(x => !x.IsSuccessful);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
                query = query.Where(x =>
                    (x.UserName != null && x.UserName.Contains(search)) ||
                    (x.TargetUserName != null && x.TargetUserName.Contains(search)) ||
                    x.Summary.Contains(search) ||
                    (x.IpAddress != null && x.IpAddress.Contains(search)));
            }

            return await query
                .OrderByDescending(x => x.OccurredAt)
                .Take(take)
                .Select(x => new SecurityAuditListItemDto
                {
                    OccurredAt = x.OccurredAt,
                    EventType = x.EventType,
                    IsSuccessful = x.IsSuccessful,
                    Summary = x.Summary,
                    UserName = x.UserName,
                    TargetUserName = x.TargetUserName,
                    IpAddress = x.IpAddress,
                    UserAgent = x.UserAgent
                })
                .ToListAsync(cancellationToken);
        }

        private static string? TrimToLength(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            return normalized.Length <= maxLength
                ? normalized
                : normalized[..maxLength];
        }
    }
}
