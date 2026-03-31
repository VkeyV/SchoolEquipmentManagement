using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Application.Services;
using SchoolEquipmentManagement.Tests.TestSupport;
using SchoolEquipmentManagement.Web.Controllers;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.Services.Equipment;
using SchoolEquipmentManagement.Web.Services.Inventory;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;
using SchoolEquipmentManagement.Web.ViewModels.Inventory;
using SchoolEquipmentManagement.Web.ViewModels.Locations;
using System.IO;
using System.Reflection;
using System.Text;

namespace SchoolEquipmentManagement.Tests.Integration
{
    public class ControllerSmokeTests
    {
        [Fact]
        public async Task EquipmentIndex_ShouldReturnViewModelWithPermissionFlags()
        {
            var equipmentService = new FakeEquipmentService
            {
                ListResult = new PagedResultDto<EquipmentListItemDto>
                {
                    Page = 1,
                    PageSize = 10,
                    TotalCount = 1,
                    Items = new List<EquipmentListItemDto>
                    {
                        new()
                        {
                            Id = 1,
                            InventoryNumber = "INV-001",
                            Name = "РќРѕСѓС‚Р±СѓРє",
                            EquipmentTypeName = "РќРѕСѓС‚Р±СѓРє",
                            EquipmentStatusName = "Р’ СЌРєСЃРїР»СѓР°С‚Р°С†РёРё",
                            LocationName = "Р“Р»Р°РІРЅС‹Р№ РєРѕСЂРїСѓСЃ, РљР°Р±РёРЅРµС‚ 101"
                        }
                    }
                }
            };

            var controller = new EquipmentController(
                equipmentService,
                new FakeEquipmentImportService(),
                new FakeUserAccessService(ModulePermission.CreateEquipment, ModulePermission.ImportEquipment),
                new FakeEquipmentLookupViewModelService(),
                new FakeEquipmentDetailsViewModelFactory(),
                new FakeEquipmentFormModelService(),
                new FakeEquipmentMediaService(),
                new EquipmentWarrantyCsvExportService())
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            var result = await controller.Index(null, null, null, null, null);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EquipmentIndexViewModel>(view.Model);
            Assert.True(model.CanCreate);
            Assert.True(model.CanImport);
            Assert.Single(model.Items);
        }

        [Fact]
        public async Task InventoryIndex_ShouldReturnViewModelWithSessions()
        {
            var inventoryService = new FakeInventoryService
            {
                Sessions = new List<InventorySessionListItemDto>
                {
                    new()
                    {
                        Id = 1,
                        Name = "Р’РµСЃРµРЅРЅСЏСЏ РёРЅРІРµРЅС‚Р°СЂРёР·Р°С†РёСЏ",
                        StartDate = new DateTime(2026, 3, 29),
                        Status = "РђРєС‚РёРІРЅР°",
                        CreatedBy = "Tester",
                        CheckedCount = 5,
                        FoundCount = 4,
                        MissingCount = 1,
                        DiscrepancyCount = 0
                    }
                }
            };
            var userAccessService = new FakeUserAccessService(ModulePermission.ViewInventory, ModulePermission.CreateInventorySession);

            var controller = new InventoryController(
                inventoryService,
                userAccessService,
                new InventoryLookupViewModelService(new FakeDictionaryService()),
                new InventoryViewModelFactory(inventoryService, userAccessService))
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InventorySessionIndexViewModel>(view.Model);
            Assert.True(model.CanCreateSession);
            Assert.Single(model.Sessions);
        }

        [Fact]
        public async Task EquipmentIssues_ShouldReturnGroupedProblemsWithQuickActions()
        {
            var equipmentService = new FakeEquipmentService
            {
                ProblemEquipmentResult = new List<EquipmentIssueItemDto>
                {
                    new()
                    {
                        EquipmentId = 7,
                        InventoryNumber = "INV-007",
                        Name = "Проблемный ноутбук",
                        EquipmentTypeName = "Ноутбук",
                        EquipmentStatusName = "Требует диагностики",
                        LocationName = "Главный корпус, Кабинет 101",
                        ResponsiblePerson = "Иванов И.И.",
                        IssueCode = "diagnostics",
                        IssueTitle = "Требует диагностики",
                        IssueDescription = "Нужна диагностика блока питания.",
                        Priority = 3,
                        PriorityLabel = "Высокий"
                    }
                }
            };

            var controller = new EquipmentController(
                equipmentService,
                new FakeEquipmentImportService(),
                new FakeUserAccessService(ModulePermission.ChangeEquipmentStatus, ModulePermission.EditEquipment),
                new FakeEquipmentLookupViewModelService(),
                new FakeEquipmentDetailsViewModelFactory(),
                new FakeEquipmentFormModelService(),
                new FakeEquipmentMediaService(),
                new EquipmentWarrantyCsvExportService())
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            var result = await controller.Issues();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EquipmentIssuesIndexViewModel>(view.Model);
            Assert.True(model.HasIssues);
            var group = Assert.Single(model.Groups);
            Assert.Equal("diagnostics", group.Code);
            var item = Assert.Single(group.Items);
            Assert.True(item.CanChangeStatus);
            Assert.True(item.CanAssignResponsible);
        }

        [Fact]
        public async Task LocationDetails_ShouldReturnEquipmentAndDiscrepancies()
        {
            var equipmentService = new FakeEquipmentService();
            equipmentService.LocationDetailsById[1] = new LocationDetailsDto
            {
                Id = 1,
                Name = "Главный корпус, Кабинет 101",
                Building = "Главный корпус",
                Room = "Кабинет 101",
                EquipmentCount = 2,
                DiscrepancyCount = 1,
                MissingCount = 1,
                LastInventoryCheckedAt = new DateTime(2026, 3, 30, 9, 30, 0, DateTimeKind.Utc),
                StatusSummary = new List<LocationStatusSummaryDto>
                {
                    new() { StatusName = "В эксплуатации", Count = 1 },
                    new() { StatusName = "В ремонте", Count = 1 }
                },
                EquipmentItems = new List<LocationEquipmentItemDto>
                {
                    new()
                    {
                        Id = 11,
                        InventoryNumber = "INV-011",
                        Name = "Учительский ноутбук",
                        EquipmentTypeName = "Ноутбук",
                        EquipmentStatusName = "В эксплуатации",
                        LastInventoryStatus = "Последняя инвентаризация подтвердила объект в этой локации."
                    },
                    new()
                    {
                        Id = 12,
                        InventoryNumber = "INV-012",
                        Name = "Проектор",
                        EquipmentTypeName = "Проектор",
                        EquipmentStatusName = "В ремонте",
                        LastInventoryStatus = "На последней инвентаризации объект не найден."
                    }
                },
                InventoryDiscrepancies = new List<LocationInventoryDiscrepancyDto>
                {
                    new()
                    {
                        EquipmentId = 12,
                        InventoryNumber = "INV-012",
                        Name = "Проектор",
                        EquipmentTypeName = "Проектор",
                        EquipmentStatusName = "В ремонте",
                        ExpectedLocationName = "Главный корпус, Кабинет 101",
                        DiscrepancyCode = "missing",
                        DiscrepancyTitle = "Не найдено в локации",
                        DiscrepancySummary = "На последней инвентаризации объект не найден.",
                        CheckedAt = new DateTime(2026, 3, 30, 9, 30, 0, DateTimeKind.Utc),
                        CheckedBy = "tester"
                    }
                }
            };

            var controller = new LocationsController(
                equipmentService,
                new FakeUserAccessService(ModulePermission.ChangeEquipmentStatus, ModulePermission.EditEquipment))
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            var result = await controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LocationDetailsViewModel>(view.Model);
            Assert.Equal("Главный корпус, Кабинет 101", model.Name);
            Assert.True(model.CanChangeStatus);
            Assert.True(model.CanAssignResponsible);
            Assert.Equal(2, model.EquipmentItems.Count);
            Assert.Single(model.InventoryDiscrepancies);
            Assert.True(model.HasInteractiveMap);
            Assert.NotEmpty(model.MapZones);
            Assert.Equal(2, model.MapMarkers.Count);
        }

        [Fact]
        public void EquipmentActions_ShouldDeclarePermissionPolicies()
        {
            AssertPermission<EquipmentController>(
                nameof(EquipmentController.Index),
                new[] { typeof(string), typeof(int?), typeof(int?), typeof(int?), typeof(string), typeof(int) },
                ModulePermission.ViewEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.WarrantyReport), new[] { typeof(string) }, ModulePermission.ViewEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.DownloadWarrantyReportCsv), new[] { typeof(string) }, ModulePermission.ViewEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Issues), Type.EmptyTypes, ModulePermission.ViewEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Import), Type.EmptyTypes, ModulePermission.ImportEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Import), new[] { typeof(EquipmentImportViewModel) }, ModulePermission.ImportEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.ApplyImport), new[] { typeof(EquipmentImportViewModel) }, ModulePermission.ImportEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Create), Type.EmptyTypes, ModulePermission.CreateEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Create), new[] { typeof(EquipmentCreateViewModel) }, ModulePermission.CreateEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Edit), new[] { typeof(int) }, ModulePermission.EditEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Edit), new[] { typeof(EquipmentEditViewModel) }, ModulePermission.EditEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.AssignResponsible), new[] { typeof(int), typeof(string) }, ModulePermission.EditEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.AssignResponsible), new[] { typeof(EquipmentAssignResponsibleViewModel) }, ModulePermission.EditEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Details), new[] { typeof(int), typeof(int) }, ModulePermission.ViewEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.Passport), new[] { typeof(int) }, ModulePermission.ViewEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.DownloadPassport), new[] { typeof(int) }, ModulePermission.ViewEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.ChangeStatus), new[] { typeof(int) }, ModulePermission.ChangeEquipmentStatus);
            AssertPermission<EquipmentController>(nameof(EquipmentController.ChangeStatus), new[] { typeof(EquipmentChangeStatusViewModel) }, ModulePermission.ChangeEquipmentStatus);
            AssertPermission<EquipmentController>(nameof(EquipmentController.ChangeLocation), new[] { typeof(int) }, ModulePermission.ChangeEquipmentLocation);
            AssertPermission<EquipmentController>(nameof(EquipmentController.ChangeLocation), new[] { typeof(EquipmentChangeLocationViewModel) }, ModulePermission.ChangeEquipmentLocation);
            AssertPermission<EquipmentController>(nameof(EquipmentController.WriteOff), new[] { typeof(int) }, ModulePermission.WriteOffEquipment);
            AssertPermission<EquipmentController>(nameof(EquipmentController.WriteOff), new[] { typeof(EquipmentWriteOffViewModel) }, ModulePermission.WriteOffEquipment);
        }

        [Fact]
        public void InventoryActions_ShouldDeclarePermissionPolicies()
        {
            AssertPermission<InventoryController>(nameof(InventoryController.Index), Type.EmptyTypes, ModulePermission.ViewInventory);
            AssertPermission<InventoryController>(nameof(InventoryController.Create), Type.EmptyTypes, ModulePermission.CreateInventorySession);
            AssertPermission<InventoryController>(nameof(InventoryController.Create), new[] { typeof(InventorySessionCreateViewModel) }, ModulePermission.CreateInventorySession);
            AssertPermission<InventoryController>(nameof(InventoryController.Details), new[] { typeof(int) }, ModulePermission.ViewInventory);
            AssertPermission<InventoryController>(nameof(InventoryController.Start), new[] { typeof(int) }, ModulePermission.ManageInventorySession);
            AssertPermission<InventoryController>(nameof(InventoryController.Complete), new[] { typeof(int) }, ModulePermission.ManageInventorySession);
            AssertPermission<InventoryController>(nameof(InventoryController.Check), new[] { typeof(int), typeof(int) }, ModulePermission.CheckInventory);
            AssertPermission<InventoryController>(nameof(InventoryController.Check), new[] { typeof(InventoryCheckViewModel) }, ModulePermission.CheckInventory);
        }

        [Fact]
        public void UsersController_ShouldDeclareManageUsersPermission()
        {
            var attribute = typeof(UsersController).GetCustomAttribute<PermissionAuthorizeAttribute>();
            Assert.NotNull(attribute);
            Assert.Equal(ModulePermission.ManageUsers, attribute!.Permission);
            Assert.Equal(PermissionPolicyNames.For(ModulePermission.ManageUsers), attribute.Policy);
        }

        [Fact]
        public void SecurityController_ShouldDeclareViewSecurityAuditPermission()
        {
            var attribute = typeof(SecurityController).GetCustomAttribute<PermissionAuthorizeAttribute>();
            Assert.NotNull(attribute);
            Assert.Equal(ModulePermission.ViewSecurityAudit, attribute!.Permission);
            Assert.Equal(PermissionPolicyNames.For(ModulePermission.ViewSecurityAudit), attribute.Policy);
        }

        [Fact]
        public void LocationsController_Details_ShouldDeclareViewEquipmentPermission()
        {
            AssertPermission<LocationsController>(nameof(LocationsController.Details), new[] { typeof(int) }, ModulePermission.ViewEquipment);
        }

        [Fact]
        public async Task DownloadWarrantyReportCsv_ShouldReturnUtf8CsvWithDetailedColumns()
        {
            var equipmentService = new FakeEquipmentService
            {
                WarrantyReportResult = new List<EquipmentWarrantyItemDto>
                {
                    new()
                    {
                        Id = 21,
                        InventoryNumber = "INV-021",
                        Name = "Учительский ноутбук",
                        EquipmentTypeName = "Ноутбук",
                        EquipmentStatusName = "В эксплуатации",
                        LocationName = "Главный корпус, Кабинет 101",
                        ResponsiblePerson = "Петров П.П.",
                        WarrantyEndDate = new DateTime(2026, 5, 15)
                    }
                }
            };

            equipmentService.DetailsById[21] = new EquipmentDetailsDto
            {
                Id = 21,
                InventoryNumber = "INV-021",
                Name = "Учительский ноутбук",
                EquipmentTypeName = "Ноутбук",
                EquipmentStatusName = "В эксплуатации",
                LocationName = "Главный корпус, Кабинет 101",
                ResponsiblePerson = "Петров П.П.",
                SerialNumber = "SN;021",
                Model = "Latitude 5530",
                Manufacturer = "Dell",
                PurchaseDate = new DateTime(2025, 4, 12),
                CommissioningDate = new DateTime(2025, 4, 18),
                WarrantyEndDate = new DateTime(2026, 5, 15),
                Notes = "Требует проверки батареи\r\nПеред летним периодом",
                History = new List<EquipmentHistoryItemDto>
                {
                    new()
                    {
                        Id = 1,
                        ActionType = "Изменение статуса",
                        ChangedField = "Статус",
                        OldValue = "На складе",
                        NewValue = "В эксплуатации",
                        Comment = "Выдан преподавателю",
                        ChangedBy = "admin",
                        ChangedAt = new DateTime(2025, 4, 18, 10, 30, 0)
                    }
                }
            };

            var controller = new EquipmentController(
                equipmentService,
                new FakeEquipmentImportService(),
                new FakeUserAccessService(ModulePermission.ViewEquipment),
                new FakeEquipmentLookupViewModelService(),
                new FakeEquipmentDetailsViewModelFactory(),
                new FakeEquipmentFormModelService(),
                new FakeEquipmentMediaService(),
                new EquipmentWarrantyCsvExportService())
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            controller.ControllerContext.HttpContext.Request.Scheme = "https";
            controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost");

            var result = await controller.DownloadWarrantyReportCsv("30");

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv; charset=utf-8", file.ContentType);
            Assert.EndsWith(".csv", file.FileDownloadName);

            var csv = Encoding.UTF8.GetString(file.FileContents);
            Assert.Contains("\"InventoryNumber\";\"Name\";\"EquipmentType\";\"Status\";\"Location\"", csv);
            Assert.Contains("\"SN;021\"", csv);
            Assert.Contains("\"Требует проверки батареи / Перед летним периодом\"", csv);
            Assert.Contains("\"https://localhost/Equipment/Details/21\"", csv);

            await using var stream = new MemoryStream(file.FileContents);
            var preview = await new EquipmentImportService(
                new FakeDictionaryService(),
                new FakeEquipmentRepository(),
                new FakeEquipmentService()).PreviewCsvAsync(stream);

            Assert.Equal(1, preview.ValidRows);
            Assert.Equal(0, preview.InvalidRows);
            var item = Assert.Single(preview.ValidItems);
            Assert.Equal("INV-021", item.InventoryNumber);
            Assert.Equal("Dell", item.Manufacturer);
        }

        private static void AssertPermission<TController>(string actionName, Type[] parameterTypes, ModulePermission expectedPermission)
        {
            var method = typeof(TController).GetMethod(actionName, BindingFlags.Instance | BindingFlags.Public, binder: null, parameterTypes, modifiers: null);
            Assert.NotNull(method);

            var attribute = method!.GetCustomAttribute<PermissionAuthorizeAttribute>();
            Assert.NotNull(attribute);
            Assert.Equal(expectedPermission, attribute!.Permission);
            Assert.Equal(PermissionPolicyNames.For(expectedPermission), attribute.Policy);
        }
    }
}
