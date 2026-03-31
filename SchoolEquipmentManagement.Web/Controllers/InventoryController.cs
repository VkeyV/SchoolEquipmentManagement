using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.Services.Inventory;
using SchoolEquipmentManagement.Web.ViewModels.Inventory;

namespace SchoolEquipmentManagement.Web.Controllers
{
    [Authorize]
    public class InventoryController : AppController
    {
        private readonly IInventoryService _inventoryService;
        private readonly IUserAccessService _userAccessService;
        private readonly IInventoryLookupViewModelService _inventoryLookupViewModelService;
        private readonly IInventoryViewModelFactory _inventoryViewModelFactory;

        public InventoryController(
            IInventoryService inventoryService,
            IUserAccessService userAccessService,
            IInventoryLookupViewModelService inventoryLookupViewModelService,
            IInventoryViewModelFactory inventoryViewModelFactory)
        {
            _inventoryService = inventoryService;
            _userAccessService = userAccessService;
            _inventoryLookupViewModelService = inventoryLookupViewModelService;
            _inventoryViewModelFactory = inventoryViewModelFactory;
        }

        [PermissionAuthorize(ModulePermission.ViewInventory)]
        public async Task<IActionResult> Index()
        {
            var viewModel = await _inventoryViewModelFactory.CreateIndexViewModelAsync();
            return View(viewModel);
        }

        [HttpGet]
        [PermissionAuthorize(ModulePermission.CreateInventorySession)]
        public IActionResult Create()
        {
            return View(new InventorySessionCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(ModulePermission.CreateInventorySession)]
        public async Task<IActionResult> Create(InventorySessionCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var sessionId = await _inventoryService.CreateSessionAsync(new CreateInventorySessionDto
                {
                    Name = viewModel.Name,
                    StartDate = viewModel.StartDate,
                    CreatedBy = _userAccessService.CurrentUserName
                });

                SetSuccessMessage("Сессия инвентаризации создана.");
                return RedirectToAction(nameof(Details), new { id = sessionId });
            }
            catch (Exception ex)
            {
                AddOperationError(ex);
                return View(viewModel);
            }
        }

        [PermissionAuthorize(ModulePermission.ViewInventory)]
        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await _inventoryViewModelFactory.CreateDetailsViewModelAsync(id);
            if (viewModel is null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(ModulePermission.ManageInventorySession)]
        public async Task<IActionResult> Start(int id)
        {
            try
            {
                await _inventoryService.StartSessionAsync(id);
                SetSuccessMessage("Сессия переведена в активное состояние.");
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(ModulePermission.ManageInventorySession)]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                await _inventoryService.CompleteSessionAsync(id);
                SetSuccessMessage("Сессия инвентаризации завершена.");
            }
            catch (Exception ex)
            {
                SetErrorMessage(ex);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        [PermissionAuthorize(ModulePermission.CheckInventory)]
        public async Task<IActionResult> Check(int sessionId, int equipmentId)
        {
            var viewModel = await _inventoryViewModelFactory.CreateCheckViewModelAsync(sessionId, equipmentId);
            if (viewModel is null)
            {
                return NotFound();
            }

            await _inventoryLookupViewModelService.PopulateLocationsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PermissionAuthorize(ModulePermission.CheckInventory)]
        public async Task<IActionResult> Check(InventoryCheckViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await _inventoryLookupViewModelService.PopulateLocationsAsync(viewModel);
                return View(viewModel);
            }

            try
            {
                await _inventoryService.RecordCheckAsync(new InventoryCheckDto
                {
                    SessionId = viewModel.SessionId,
                    EquipmentId = viewModel.EquipmentId,
                    IsFound = viewModel.IsFound,
                    ActualLocationId = viewModel.IsFound ? viewModel.ActualLocationId : null,
                    ConditionComment = viewModel.ConditionComment,
                    CheckedBy = _userAccessService.CurrentUserName
                });

                SetSuccessMessage("Результат проверки сохранен.");
                return RedirectToAction(nameof(Details), new { id = viewModel.SessionId });
            }
            catch (Exception ex)
            {
                AddOperationError(ex);
                await _inventoryLookupViewModelService.PopulateLocationsAsync(viewModel);
                return View(viewModel);
            }
        }
    }
}
