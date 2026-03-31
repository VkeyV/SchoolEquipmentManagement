using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Fluent;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Web.Documents;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.Services.Equipment;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;
using System.Text.Json;

namespace SchoolEquipmentManagement.Web.Controllers;

[Authorize]
public class EquipmentController : AppController
{
    private const int IndexPageSize = 10;

    private readonly IEquipmentService _equipmentService;
    private readonly IEquipmentImportService _equipmentImportService;
    private readonly IUserAccessService _userAccessService;
    private readonly IEquipmentLookupViewModelService _lookupService;
    private readonly IEquipmentDetailsViewModelFactory _detailsViewModelFactory;
    private readonly IEquipmentFormModelService _formModelService;
    private readonly IEquipmentMediaService _equipmentMediaService;
    private readonly IEquipmentWarrantyCsvExportService _equipmentWarrantyCsvExportService;

    public EquipmentController(
        IEquipmentService equipmentService,
        IEquipmentImportService equipmentImportService,
        IUserAccessService userAccessService,
        IEquipmentLookupViewModelService lookupService,
        IEquipmentDetailsViewModelFactory detailsViewModelFactory,
        IEquipmentFormModelService formModelService,
        IEquipmentMediaService equipmentMediaService,
        IEquipmentWarrantyCsvExportService equipmentWarrantyCsvExportService)
    {
        _equipmentService = equipmentService;
        _equipmentImportService = equipmentImportService;
        _userAccessService = userAccessService;
        _lookupService = lookupService;
        _detailsViewModelFactory = detailsViewModelFactory;
        _formModelService = formModelService;
        _equipmentMediaService = equipmentMediaService;
        _equipmentWarrantyCsvExportService = equipmentWarrantyCsvExportService;
    }

    [PermissionAuthorize(ModulePermission.ViewEquipment)]
    public async Task<IActionResult> Index(string? search, int? typeId, int? statusId, int? locationId, string? warrantyFilter, int page = 1)
    {
        var result = await _equipmentService.GetEquipmentListAsync(new EquipmentFilterDto
        {
            Search = search,
            TypeId = typeId,
            StatusId = statusId,
            LocationId = locationId,
            WarrantyFilter = warrantyFilter,
            Page = page,
            PageSize = IndexPageSize
        });

        var viewModel = CreateIndexViewModel(search, typeId, statusId, locationId, warrantyFilter, result);

        await _lookupService.PopulateIndexAsync(viewModel);
        viewModel.WarrantyFilters = BuildWarrantyFilterOptions(viewModel.WarrantyFilter);
        return View(viewModel);
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ViewEquipment)]
    public async Task<IActionResult> WarrantyReport(string? warrantyFilter = null)
    {
        var report = await _equipmentService.GetWarrantyReportAsync(new EquipmentWarrantyFilterDto
        {
            WarrantyFilter = warrantyFilter
        });

        return View(new EquipmentWarrantyReportViewModel
        {
            WarrantyFilter = warrantyFilter,
            WarrantyFilters = BuildWarrantyFilterOptions(warrantyFilter),
            Items = report.Select(item => new EquipmentWarrantyReportItemViewModel
            {
                Id = item.Id,
                InventoryNumber = item.InventoryNumber,
                Name = item.Name,
                EquipmentType = item.EquipmentTypeName,
                Status = item.EquipmentStatusName,
                Location = item.LocationName,
                ResponsiblePerson = item.ResponsiblePerson,
                WarrantyEndDate = item.WarrantyEndDate,
                WarrantyDaysLeft = item.WarrantyDaysLeft
            }).ToList()
        });
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ViewEquipment)]
    public async Task<IActionResult> DownloadWarrantyReportCsv(string? warrantyFilter = null)
    {
        var report = await _equipmentService.GetWarrantyReportAsync(new EquipmentWarrantyFilterDto
        {
            WarrantyFilter = warrantyFilter
        });

        var items = await BuildWarrantyExportDetailsAsync(report);
        var fileContents = _equipmentWarrantyCsvExportService.Export(items, BuildEquipmentDetailsUrl);
        var fileName = BuildWarrantyImportCsvFileName(warrantyFilter);

        return File(fileContents, "text/csv; charset=utf-8", fileName);
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ViewEquipment)]
    public async Task<IActionResult> Issues()
    {
        var issues = await _equipmentService.GetProblemEquipmentAsync();
        return View(CreateIssuesViewModel(issues));
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.EditEquipment)]
    public async Task<IActionResult> AssignResponsible(int id, string? returnUrl = null)
    {
        var item = await _equipmentService.GetEquipmentDetailsAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(new EquipmentAssignResponsibleViewModel
        {
            EquipmentId = item.Id,
            InventoryNumber = item.InventoryNumber,
            Name = item.Name,
            CurrentResponsiblePerson = item.ResponsiblePerson,
            ResponsiblePerson = item.ResponsiblePerson ?? string.Empty,
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(ModulePermission.EditEquipment)]
    public async Task<IActionResult> AssignResponsible(EquipmentAssignResponsibleViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            await _equipmentService.AssignResponsibleAsync(new AssignEquipmentResponsibleDto
            {
                EquipmentId = viewModel.EquipmentId,
                ResponsiblePerson = viewModel.ResponsiblePerson,
                Comment = viewModel.Comment,
                ChangedBy = _userAccessService.CurrentUserName
            });

            SetSuccessMessage("Ответственное лицо обновлено.");
            return RedirectToEquipmentDestination(viewModel.ReturnUrl, viewModel.EquipmentId);
        }
        catch (Exception ex)
        {
            AddOperationError(ex);
            return View(viewModel);
        }
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ImportEquipment)]
    public IActionResult Import()
    {
        return View(new EquipmentImportViewModel());
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ImportEquipment)]
    public async Task<IActionResult> DownloadImportCsv()
    {
        var result = await _equipmentService.GetEquipmentListAsync(new EquipmentFilterDto
        {
            Page = 1,
            PageSize = int.MaxValue
        });

        var items = await BuildEquipmentExportDetailsAsync(result.Items.Select(item => item.Id));
        var fileContents = _equipmentWarrantyCsvExportService.Export(items, BuildEquipmentDetailsUrl);
        var fileName = _equipmentMediaService.SanitizeFileName($"equipment-import-ready-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

        return File(fileContents, "text/csv; charset=utf-8", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(ModulePermission.ImportEquipment)]
    public async Task<IActionResult> Import(EquipmentImportViewModel viewModel)
    {
        if (viewModel.File is null || viewModel.File.Length == 0)
        {
            ModelState.AddModelError(nameof(viewModel.File), "Выберите CSV-файл для импорта.");
            return View(viewModel);
        }

        if (!string.Equals(Path.GetExtension(viewModel.File.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(viewModel.File), "Поддерживается только CSV-файл с разделителем ';'.");
            return View(viewModel);
        }

        try
        {
            await using var stream = viewModel.File.OpenReadStream();
            var preview = await _equipmentImportService.PreviewCsvAsync(stream);

            viewModel.HasPreview = true;
            viewModel.TotalRows = preview.TotalRows;
            viewModel.ValidRows = preview.ValidRows;
            viewModel.InvalidRows = preview.InvalidRows;
            viewModel.PayloadJson = JsonSerializer.Serialize(preview.ValidItems);
            viewModel.PreviewRows = preview.Rows.Select(row => new EquipmentImportPreviewRowViewModel
            {
                RowNumber = row.RowNumber,
                InventoryNumber = row.InventoryNumber,
                Name = row.Name,
                EquipmentTypeName = row.EquipmentTypeName,
                StatusName = row.StatusName,
                LocationName = row.LocationName,
                Errors = row.Errors,
                IsValid = row.IsValid
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            AddOperationError(ex);
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(ModulePermission.ImportEquipment)]
    public async Task<IActionResult> ApplyImport(EquipmentImportViewModel viewModel)
    {
        try
        {
            var items = JsonSerializer.Deserialize<List<EquipmentImportApplyItemDto>>(viewModel.PayloadJson)
                ?? new List<EquipmentImportApplyItemDto>();

            var result = await _equipmentImportService.ImportAsync(items, _userAccessService.CurrentUserName);
            SetSuccessMessage($"Импорт завершен. Добавлено записей: {result.ImportedCount}.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            AddOperationError(ex);
            viewModel.HasPreview = true;
            return View("Import", viewModel);
        }
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.CreateEquipment)]
    public async Task<IActionResult> Create()
    {
        var viewModel = new EquipmentCreateViewModel();
        await _lookupService.PopulateFormAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(ModulePermission.CreateEquipment)]
    public async Task<IActionResult> Create(EquipmentCreateViewModel viewModel)
    {
        viewModel = _formModelService.HydrateCreateModel(viewModel, Request.Form, ModelState);
        _formModelService.ValidateEquipmentModel(viewModel, ModelState);
        _formModelService.ValidatePhoto(viewModel.Photo, ModelState);

        if (!ModelState.IsValid)
        {
            return await ReturnCreateViewAsync(viewModel);
        }

        try
        {
            var createdId = await _equipmentService.CreateEquipmentAsync(
                _formModelService.CreateCreateDto(viewModel, _userAccessService.CurrentUserName));

            if (viewModel.Photo is not null)
            {
                await _equipmentMediaService.SavePhotoAsync(createdId, viewModel.Photo);
            }

            SetSuccessMessage($"Оборудование успешно добавлено. ID = {createdId}");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            AddOperationError(ex);
            return await ReturnCreateViewAsync(viewModel);
        }
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.EditEquipment)]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _equipmentService.GetEquipmentDetailsAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var viewModel = CreateEditViewModel(item);

        await _lookupService.PopulateFormAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(ModulePermission.EditEquipment)]
    public async Task<IActionResult> Edit(EquipmentEditViewModel viewModel)
    {
        viewModel = _formModelService.HydrateEditModel(viewModel, Request.Form, ModelState);
        _formModelService.ValidateEquipmentModel(viewModel, ModelState);
        _formModelService.ValidatePhoto(viewModel.Photo, ModelState);
        viewModel.ExistingPhotoUrl = _equipmentMediaService.ResolvePhotoSource(
            viewModel.Id,
            viewModel.Name,
            viewModel.Model ?? "IT",
            viewModel.InventoryNumber,
            preferUploadedFileOnly: true);

        if (!ModelState.IsValid)
        {
            return await ReturnEditViewAsync(viewModel);
        }

        try
        {
            await _equipmentService.UpdateEquipmentAsync(
                _formModelService.CreateUpdateDto(viewModel, _userAccessService.CurrentUserName));

            SetSuccessMessage("Карточка оборудования успешно обновлена.");

            if (viewModel.RemovePhoto)
            {
                _equipmentMediaService.RemovePhoto(viewModel.Id);
            }

            if (viewModel.Photo is not null)
            {
                await _equipmentMediaService.SavePhotoAsync(viewModel.Id, viewModel.Photo);
            }

            return RedirectToAction(nameof(Details), new { id = viewModel.Id });
        }
        catch (Exception ex)
        {
            AddOperationError(ex);
            return await ReturnEditViewAsync(viewModel);
        }
    }

    [PermissionAuthorize(ModulePermission.ViewEquipment)]
    public async Task<IActionResult> Details(int id, int historyPage = 1)
    {
        var detailsViewModel = await CreateDetailsViewModelAsync(id, historyPage);

        return detailsViewModel is null ? NotFound() : View(detailsViewModel);
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ViewEquipment)]
    public async Task<IActionResult> Passport(int id)
    {
        var detailsViewModel = await CreateDetailsViewModelAsync(id, includeFullHistory: true);

        return detailsViewModel is null ? NotFound() : View(detailsViewModel);
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ViewEquipment)]
    public async Task<IActionResult> DownloadPassport(int id)
    {
        var detailsViewModel = await CreateDetailsViewModelAsync(id, includeFullHistory: true);

        if (detailsViewModel is null)
        {
            return NotFound();
        }

        var photoBytes = _equipmentMediaService.GetPhotoBytes(id);
        var qrBytes = _equipmentMediaService.BuildQrCodeBytes(BuildEquipmentDetailsUrl(id));
        var document = new EquipmentPassportPdfDocument(detailsViewModel, photoBytes, qrBytes);
        var fileName = $"passport-{_equipmentMediaService.SanitizeFileName(detailsViewModel.InventoryNumber)}.pdf";
        return File(document.GeneratePdf(), "application/pdf", fileName);
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ChangeEquipmentStatus)]
    public async Task<IActionResult> ChangeStatus(int id)
    {
        var item = await _equipmentService.GetEquipmentDetailsAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var viewModel = CreateStatusViewModel(item);

        await _lookupService.PopulateStatusOptionsAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(ModulePermission.ChangeEquipmentStatus)]
    public async Task<IActionResult> ChangeStatus(EquipmentChangeStatusViewModel viewModel)
    {
        if (viewModel.NewStatusId.HasValue && viewModel.NewStatusId.Value == viewModel.CurrentStatusId)
        {
            ModelState.AddModelError(nameof(viewModel.NewStatusId), "Выберите статус, отличный от текущего.");
        }

        if (!ModelState.IsValid)
        {
            return await ReturnStatusViewAsync(viewModel);
        }

        try
        {
            await _equipmentService.ChangeStatusAsync(new ChangeEquipmentStatusDto
            {
                EquipmentId = viewModel.EquipmentId,
                NewStatusId = viewModel.NewStatusId!.Value,
                Comment = viewModel.Comment,
                ChangedBy = _userAccessService.CurrentUserName
            });

            SetSuccessMessage("Статус оборудования успешно изменен.");
            return RedirectToAction(nameof(Details), new { id = viewModel.EquipmentId });
        }
        catch (Exception ex)
        {
            AddOperationError(ex);
            return await ReturnStatusViewAsync(viewModel);
        }
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.ChangeEquipmentLocation)]
    public async Task<IActionResult> ChangeLocation(int id)
    {
        var item = await _equipmentService.GetEquipmentDetailsAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var viewModel = CreateLocationViewModel(item);

        await _lookupService.PopulateLocationOptionsAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(ModulePermission.ChangeEquipmentLocation)]
    public async Task<IActionResult> ChangeLocation(EquipmentChangeLocationViewModel viewModel)
    {
        if (viewModel.NewLocationId.HasValue && viewModel.NewLocationId.Value == viewModel.CurrentLocationId)
        {
            ModelState.AddModelError(nameof(viewModel.NewLocationId), "Выберите местоположение, отличное от текущего.");
        }

        if (!ModelState.IsValid)
        {
            return await ReturnLocationViewAsync(viewModel);
        }

        try
        {
            await _equipmentService.ChangeLocationAsync(new ChangeEquipmentLocationDto
            {
                EquipmentId = viewModel.EquipmentId,
                NewLocationId = viewModel.NewLocationId!.Value,
                Comment = viewModel.Comment,
                ChangedBy = _userAccessService.CurrentUserName
            });

            SetSuccessMessage("Местоположение оборудования успешно изменено.");
            return RedirectToAction(nameof(Details), new { id = viewModel.EquipmentId });
        }
        catch (Exception ex)
        {
            AddOperationError(ex);
            return await ReturnLocationViewAsync(viewModel);
        }
    }

    [HttpGet]
    [PermissionAuthorize(ModulePermission.WriteOffEquipment)]
    public async Task<IActionResult> WriteOff(int id)
    {
        var item = await _equipmentService.GetEquipmentDetailsAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        if (string.Equals(item.EquipmentStatusName, "Списано", StringComparison.OrdinalIgnoreCase))
        {
            SetErrorMessage("Оборудование уже списано.");
            return RedirectToAction(nameof(Details), new { id });
        }

        var viewModel = CreateWriteOffViewModel(item);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermissionAuthorize(ModulePermission.WriteOffEquipment)]
    public async Task<IActionResult> WriteOff(EquipmentWriteOffViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var writtenOffStatusId = await _lookupService.GetWrittenOffStatusIdAsync();

            await _equipmentService.WriteOffAsync(new WriteOffEquipmentDto
            {
                EquipmentId = viewModel.EquipmentId,
                WrittenOffStatusId = writtenOffStatusId,
                Comment = viewModel.Comment,
                ChangedBy = _userAccessService.CurrentUserName
            });

            SetSuccessMessage("Оборудование успешно списано.");
            return RedirectToAction(nameof(Details), new { id = viewModel.EquipmentId });
        }
        catch (Exception ex)
        {
            AddOperationError(ex);
            return View(viewModel);
        }
    }

    private EquipmentIndexViewModel CreateIndexViewModel(
        string? search,
        int? typeId,
        int? statusId,
        int? locationId,
        string? warrantyFilter,
        PagedResultDto<EquipmentListItemDto> result)
    {
        return new EquipmentIndexViewModel
        {
            Search = search,
            TypeId = typeId,
            StatusId = statusId,
            LocationId = locationId,
            WarrantyFilter = warrantyFilter,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages,
            CanCreate = _userAccessService.HasPermission(ModulePermission.CreateEquipment),
            CanImport = _userAccessService.HasPermission(ModulePermission.ImportEquipment),
            Items = result.Items.Select(item => new EquipmentListViewModel
            {
                Id = item.Id,
                LocationId = item.LocationId,
                InventoryNumber = item.InventoryNumber,
                Name = item.Name,
                EquipmentType = item.EquipmentTypeName,
                Status = item.EquipmentStatusName,
                Location = item.LocationName,
                ResponsiblePerson = item.ResponsiblePerson,
                WarrantyEndDate = item.WarrantyEndDate,
                WarrantyDaysLeft = item.WarrantyDaysLeft
            }).ToList()
        };
    }

    private EquipmentIssuesIndexViewModel CreateIssuesViewModel(IReadOnlyList<EquipmentIssueItemDto> issues)
    {
        return new EquipmentIssuesIndexViewModel
        {
            Groups = GetIssueGroups()
                .Select(group => CreateIssueGroup(group.Code, group.Title, group.Description, issues))
                .Where(group => group.Count > 0)
                .ToList()
        };
    }

    private EquipmentEditViewModel CreateEditViewModel(EquipmentDetailsDto item)
    {
        return new EquipmentEditViewModel
        {
            Id = item.Id,
            InventoryNumber = item.InventoryNumber,
            Name = item.Name,
            SerialNumber = item.SerialNumber,
            Manufacturer = item.Manufacturer,
            Model = item.Model,
            PurchaseDate = item.PurchaseDate,
            CommissioningDate = item.CommissioningDate,
            WarrantyEndDate = item.WarrantyEndDate,
            ResponsiblePerson = item.ResponsiblePerson,
            EquipmentTypeId = item.EquipmentTypeId,
            EquipmentStatusId = item.EquipmentStatusId,
            LocationId = item.LocationId,
            Notes = item.Notes,
            ExistingPhotoUrl = ResolveExistingPhotoUrl(item.Id, item.Name, item.EquipmentTypeName, item.InventoryNumber)
        };
    }

    private static EquipmentChangeStatusViewModel CreateStatusViewModel(EquipmentDetailsDto item)
    {
        return new EquipmentChangeStatusViewModel
        {
            EquipmentId = item.Id,
            InventoryNumber = item.InventoryNumber,
            Name = item.Name,
            CurrentStatusId = item.EquipmentStatusId,
            CurrentStatus = item.EquipmentStatusName
        };
    }

    private static EquipmentChangeLocationViewModel CreateLocationViewModel(EquipmentDetailsDto item)
    {
        return new EquipmentChangeLocationViewModel
        {
            EquipmentId = item.Id,
            InventoryNumber = item.InventoryNumber,
            Name = item.Name,
            CurrentLocationId = item.LocationId,
            CurrentLocation = item.LocationName
        };
    }

    private static EquipmentWriteOffViewModel CreateWriteOffViewModel(EquipmentDetailsDto item)
    {
        return new EquipmentWriteOffViewModel
        {
            EquipmentId = item.Id,
            InventoryNumber = item.InventoryNumber,
            Name = item.Name,
            CurrentStatus = item.EquipmentStatusName
        };
    }

    private async Task<EquipmentDetailsViewModel?> CreateDetailsViewModelAsync(
        int id,
        int historyPage = 1,
        bool includeFullHistory = false)
    {
        return await _detailsViewModelFactory.CreateAsync(
            id,
            historyPage,
            includeFullHistory,
            BuildEquipmentDetailsUrl);
    }

    private async Task<IActionResult> ReturnCreateViewAsync(EquipmentCreateViewModel viewModel)
    {
        await _lookupService.PopulateFormAsync(viewModel);
        return View(viewModel);
    }

    private async Task<IActionResult> ReturnEditViewAsync(EquipmentEditViewModel viewModel)
    {
        await _lookupService.PopulateFormAsync(viewModel);
        return View(viewModel);
    }

    private async Task<IActionResult> ReturnStatusViewAsync(EquipmentChangeStatusViewModel viewModel)
    {
        await _lookupService.PopulateStatusOptionsAsync(viewModel);
        return View(viewModel);
    }

    private async Task<IActionResult> ReturnLocationViewAsync(EquipmentChangeLocationViewModel viewModel)
    {
        await _lookupService.PopulateLocationOptionsAsync(viewModel);
        return View(viewModel);
    }

    private string ResolveExistingPhotoUrl(int equipmentId, string equipmentName, string equipmentTypeName, string inventoryNumber)
    {
        return _equipmentMediaService.ResolvePhotoSource(
            equipmentId,
            equipmentName,
            equipmentTypeName,
            inventoryNumber,
            preferUploadedFileOnly: true);
    }

    private string BuildEquipmentDetailsUrl(int equipmentId)
    {
        var detailsPath = $"/Equipment/Details/{equipmentId}";
        var generatedUrl = Url is null
            ? null
            : Url.Action(nameof(Details), "Equipment", new { id = equipmentId }, Request.Scheme);

        if (!string.IsNullOrWhiteSpace(generatedUrl))
        {
            return generatedUrl;
        }

        return string.IsNullOrWhiteSpace(Request.Host.Value)
            ? detailsPath
            : $"{Request.Scheme}://{Request.Host}{detailsPath}";
    }

    private async Task<IReadOnlyList<EquipmentDetailsDto>> BuildWarrantyExportDetailsAsync(
        IReadOnlyList<EquipmentWarrantyItemDto> reportItems)
    {
        var fallbackItems = reportItems.ToDictionary(item => item.Id, CreateFallbackWarrantyDetails);
        return await BuildEquipmentExportDetailsAsync(reportItems.Select(item => item.Id), fallbackItems);
    }

    private static EquipmentDetailsDto CreateFallbackWarrantyDetails(EquipmentWarrantyItemDto item)
    {
        return new EquipmentDetailsDto
        {
            Id = item.Id,
            InventoryNumber = item.InventoryNumber,
            Name = item.Name,
            EquipmentTypeName = item.EquipmentTypeName,
            EquipmentStatusName = item.EquipmentStatusName,
            LocationName = item.LocationName,
            ResponsiblePerson = item.ResponsiblePerson,
            WarrantyEndDate = item.WarrantyEndDate
        };
    }

    private async Task<IReadOnlyList<EquipmentDetailsDto>> BuildEquipmentExportDetailsAsync(
        IEnumerable<int> equipmentIds,
        IReadOnlyDictionary<int, EquipmentDetailsDto>? fallbackItems = null)
    {
        var ids = equipmentIds
            .Distinct()
            .ToList();

        var detailTasks = ids
            .Select(id => _equipmentService.GetEquipmentDetailsAsync(id))
            .ToArray();

        var details = await Task.WhenAll(detailTasks);

        var exportItems = new List<EquipmentDetailsDto>(ids.Count);
        for (var index = 0; index < ids.Count; index++)
        {
            var detail = details[index];
            if (detail is not null)
            {
                exportItems.Add(detail);
                continue;
            }

            if (fallbackItems is not null && fallbackItems.TryGetValue(ids[index], out var fallbackItem))
            {
                exportItems.Add(fallbackItem);
            }
        }

        return exportItems;
    }

    private string BuildWarrantyImportCsvFileName(string? warrantyFilter)
    {
        var filterSuffix = warrantyFilter switch
        {
            "30" => "30-days",
            "60" => "60-days",
            "90" => "90-days",
            "expired" => "expired",
            _ => "all"
        };

        var fileName = $"warranty-import-{filterSuffix}-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
        return _equipmentMediaService.SanitizeFileName(fileName);
    }

    private EquipmentIssueGroupViewModel CreateIssueGroup(
        string code,
        string title,
        string description,
        IReadOnlyList<EquipmentIssueItemDto> issues)
    {
        var normalizedTitle = string.Equals(code, "repair", StringComparison.Ordinal)
            ? "В ремонте"
            : title;

        return new EquipmentIssueGroupViewModel
        {
            Code = code,
            Title = normalizedTitle,
            Description = description,
            Items = issues
                .Where(issue => string.Equals(issue.IssueCode, code, StringComparison.Ordinal))
                .Select(MapIssueItem)
                .OrderByDescending(item => GetPriorityRank(item.PriorityLabel))
                .ThenBy(item => item.InventoryNumber)
                .ToList()
        };
    }

    private EquipmentIssueListItemViewModel MapIssueItem(EquipmentIssueItemDto issue)
    {
        return new EquipmentIssueListItemViewModel
        {
            EquipmentId = issue.EquipmentId,
            InventoryNumber = issue.InventoryNumber,
            Name = issue.Name,
            EquipmentType = issue.EquipmentTypeName,
            Status = issue.EquipmentStatusName,
            Location = issue.LocationName,
            ResponsiblePerson = issue.ResponsiblePerson,
            IssueDescription = issue.IssueDescription,
            PriorityLabel = issue.PriorityLabel,
            LastCheckedAt = issue.LastCheckedAt,
            LastCheckedBy = issue.LastCheckedBy,
            ActualLocationName = issue.ActualLocationName,
            CanChangeStatus = _userAccessService.HasPermission(ModulePermission.ChangeEquipmentStatus),
            CanAssignResponsible = _userAccessService.HasPermission(ModulePermission.EditEquipment)
        };
    }

    private IActionResult RedirectToEquipmentDestination(string? returnUrl, int equipmentId)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { id = equipmentId });
    }

    private static IReadOnlyList<(string Code, string Title, string Description)> GetIssueGroups()
    {
        return
        [
            ("repair", "На ремонте", "Объекты, которые выведены из эксплуатации и требуют ремонта."),
            ("diagnostics", "Требует диагностики", "Оборудование, по которому нужно принять решение после диагностики."),
            ("missing", "Не найдено", "Объекты, не подтвержденные последней инвентаризацией."),
            ("discrepancy", "Найдено с расхождением", "Оборудование найдено, но фактическое местоположение не совпадает с учетным.")
        ];
    }

    private static int GetPriorityRank(string priorityLabel)
    {
        return priorityLabel switch
        {
            "Критичный" => 4,
            "Высокий" => 3,
            "Средний" => 2,
            _ => 1
        };
    }

    private static List<SelectListItem> BuildWarrantyFilterOptions(string? selectedFilter)
    {
        return
        [
            CreateWarrantyFilterOption(null, "Все гарантии", selectedFilter),
            CreateWarrantyFilterOption("30", "Истекает до 30 дней", selectedFilter),
            CreateWarrantyFilterOption("60", "Истекает до 60 дней", selectedFilter),
            CreateWarrantyFilterOption("90", "Истекает до 90 дней", selectedFilter),
            CreateWarrantyFilterOption("expired", "Уже истекла", selectedFilter)
        ];
    }

    private static SelectListItem CreateWarrantyFilterOption(string? value, string text, string? selectedFilter)
    {
        return new SelectListItem
        {
            Value = value,
            Text = text,
            Selected = string.Equals(value, selectedFilter, StringComparison.OrdinalIgnoreCase)
        };
    }
}
