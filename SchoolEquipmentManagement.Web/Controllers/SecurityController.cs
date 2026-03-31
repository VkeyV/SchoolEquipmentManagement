using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.ViewModels.Security;

namespace SchoolEquipmentManagement.Web.Controllers
{
    [Authorize]
    [PermissionAuthorize(ModulePermission.ViewSecurityAudit)]
    public class SecurityController : Controller
    {
        private readonly ISecurityAuditService _securityAuditService;
        private readonly SecurityOptions _securityOptions;

        public SecurityController(
            ISecurityAuditService securityAuditService,
            IOptions<SecurityOptions> securityOptions)
        {
            _securityAuditService = securityAuditService;
            _securityOptions = securityOptions.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, bool failuresOnly = false, CancellationToken cancellationToken = default)
        {
            var items = await _securityAuditService.GetRecentAsync(
                new SecurityAuditFilterDto
                {
                    Search = search,
                    FailuresOnly = failuresOnly,
                    Take = Math.Max(20, _securityOptions.AuditPageSize)
                },
                cancellationToken);

            var viewModel = new SecurityAuditIndexViewModel
            {
                Search = search,
                FailuresOnly = failuresOnly,
                Items = items
                    .Select(item => new SecurityAuditItemViewModel
                    {
                        OccurredAt = item.OccurredAt,
                        EventDisplayName = SecurityAuditPresentation.GetEventDisplayName(item.EventType),
                        Summary = item.Summary,
                        UserName = item.UserName,
                        TargetUserName = item.TargetUserName,
                        IpAddress = item.IpAddress,
                        UserAgent = item.UserAgent,
                        IsSuccessful = item.IsSuccessful
                    })
                    .ToList()
            };

            return View(viewModel);
        }
    }
}
