using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Domain.Enums;
using SchoolEquipmentManagement.Domain.Exceptions;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.ViewModels.Users;

namespace SchoolEquipmentManagement.Web.Controllers
{
    [Authorize]
    [PermissionAuthorize(ModulePermission.ManageUsers)]
    public class UsersController : AppController
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IUserAccessService _userAccessService;

        public UsersController(
            IUserManagementService userManagementService,
            IUserAccessService userAccessService)
        {
            _userManagementService = userManagementService;
            _userAccessService = userAccessService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManagementService.GetUsersAsync();
            var viewModel = new UserIndexViewModel
            {
                Items = users
                    .Select(user => new UserListItemViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        DisplayName = user.DisplayName,
                        Email = user.Email,
                        RoleDisplayName = UserPermissionMatrix.GetRoleDisplayName(user.Role),
                        IsActive = user.IsActive,
                        TwoFactorEnabled = user.TwoFactorEnabled,
                        LastSignInAt = user.LastSignInAt,
                        LockoutEndUtc = user.LockoutEndUtc,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new UserCreateViewModel
            {
                Role = UserRole.Viewer,
                IsActive = true
            };

            PopulateRoleOptions(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                PopulateRoleOptions(viewModel);
                return View(viewModel);
            }

            try
            {
                await _userManagementService.CreateUserAsync(new CreateUserDto
                {
                    UserName = viewModel.UserName,
                    DisplayName = viewModel.DisplayName,
                    Email = viewModel.Email,
                    Password = viewModel.Password,
                    Role = viewModel.Role!.Value,
                    IsActive = viewModel.IsActive,
                    TwoFactorEnabled = viewModel.TwoFactorEnabled,
                    PerformedByUserName = _userAccessService.CurrentUserName
                });

                SetSuccessMessage("Учетная запись создана.");
                return RedirectToAction(nameof(Index));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                PopulateRoleOptions(viewModel);
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManagementService.GetUserForEditAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                TwoFactorEnabled = user.TwoFactorEnabled
            };

            PopulateRoleOptions(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                PopulateRoleOptions(viewModel);
                return View(viewModel);
            }

            try
            {
                await _userManagementService.UpdateUserAsync(new UpdateUserDto
                {
                    Id = viewModel.Id,
                    DisplayName = viewModel.DisplayName,
                    Email = viewModel.Email,
                    Role = viewModel.Role!.Value,
                    IsActive = viewModel.IsActive,
                    TwoFactorEnabled = viewModel.TwoFactorEnabled,
                    NewPassword = viewModel.NewPassword,
                    PerformedByUserName = _userAccessService.CurrentUserName
                });

                SetSuccessMessage("Учетная запись обновлена.");
                return RedirectToAction(nameof(Index));
            }
            catch (DomainException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                PopulateRoleOptions(viewModel);
                return View(viewModel);
            }
        }

        private static void PopulateRoleOptions(UserCreateViewModel viewModel)
        {
            viewModel.Roles = BuildRoleOptions(viewModel.Role);
        }

        private static void PopulateRoleOptions(UserEditViewModel viewModel)
        {
            viewModel.Roles = BuildRoleOptions(viewModel.Role);
        }

        private static List<SelectListItem> BuildRoleOptions(UserRole? selectedRole)
        {
            return Enum.GetValues<UserRole>()
                .Select(role => new SelectListItem
                {
                    Value = role.ToString(),
                    Text = UserPermissionMatrix.GetRoleDisplayName(role),
                    Selected = role == selectedRole
                })
                .ToList();
        }
    }
}
