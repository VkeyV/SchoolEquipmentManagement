using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Tests.TestSupport;
using SchoolEquipmentManagement.Web.Controllers;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.Services.Equipment;
using SchoolEquipmentManagement.Web.Services.Inventory;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;
using SchoolEquipmentManagement.Web.ViewModels.Inventory;

namespace SchoolEquipmentManagement.Tests.Integration
{
    public class UserScenarioTests
    {
        [Fact]
        public async Task EquipmentCreate_Post_ShouldCreateEquipmentSavePhotoAndRedirectToIndex()
        {
            var equipmentService = new FakeEquipmentService { CreatedId = 41 };
            var mediaService = new FakeEquipmentMediaService();
            var lookupService = new FakeEquipmentLookupViewModelService();
            var controller = new EquipmentController(
                equipmentService,
                new FakeEquipmentImportService(),
                new FakeUserAccessService(ModulePermission.CreateEquipment),
                lookupService,
                new FakeEquipmentDetailsViewModelFactory(),
                new EquipmentFormModelService(),
                mediaService,
                new EquipmentWarrantyCsvExportService())
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            AttachForm(controller, CreateEquipmentForm("teacher-station.png"));

            var result = await controller.Create(new EquipmentCreateViewModel());

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(EquipmentController.Index), redirect.ActionName);
            Assert.NotNull(equipmentService.LastCreateDto);
            Assert.Equal("INV-500", equipmentService.LastCreateDto!.InventoryNumber);
            Assert.Equal("Рабочая станция преподавателя", equipmentService.LastCreateDto.Name);
            Assert.Equal("TestUser", equipmentService.LastCreateDto.ChangedBy);
            Assert.Equal(41, mediaService.LastSavedEquipmentId);
            Assert.NotNull(mediaService.LastSavedPhoto);
            Assert.Equal("Оборудование успешно добавлено. ID = 41", controller.TempData["SuccessMessage"]);
            Assert.Equal(0, lookupService.PopulateFormCalls);
        }

        [Fact]
        public async Task EquipmentChangeStatus_Post_ShouldReturnView_WhenSameStatusSelected()
        {
            var equipmentService = new FakeEquipmentService();
            var lookupService = new FakeEquipmentLookupViewModelService();
            var controller = new EquipmentController(
                equipmentService,
                new FakeEquipmentImportService(),
                new FakeUserAccessService(ModulePermission.ChangeEquipmentStatus),
                lookupService,
                new FakeEquipmentDetailsViewModelFactory(),
                new FakeEquipmentFormModelService(),
                new FakeEquipmentMediaService(),
                new EquipmentWarrantyCsvExportService())
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            var model = new EquipmentChangeStatusViewModel
            {
                EquipmentId = 7,
                CurrentStatusId = 2,
                NewStatusId = 2
            };

            var result = await controller.ChangeStatus(model);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<EquipmentChangeStatusViewModel>(view.Model);
            Assert.Same(model, returnedModel);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState[nameof(EquipmentChangeStatusViewModel.NewStatusId)]!.Errors,
                x => x.ErrorMessage == "Выберите статус, отличный от текущего.");
            Assert.Equal(1, lookupService.PopulateStatusOptionsCalls);
            Assert.Null(equipmentService.LastChangeStatusDto);
        }

        [Fact]
        public async Task EquipmentWriteOff_Get_ShouldRedirectToDetails_WhenItemAlreadyWrittenOff()
        {
            var equipmentService = new FakeEquipmentService
            {
                DetailsResult = new EquipmentDetailsDto
                {
                    Id = 15,
                    InventoryNumber = "INV-015",
                    Name = "Старый монитор",
                    EquipmentStatusName = "Списано"
                }
            };
            var controller = new EquipmentController(
                equipmentService,
                new FakeEquipmentImportService(),
                new FakeUserAccessService(ModulePermission.WriteOffEquipment),
                new FakeEquipmentLookupViewModelService(),
                new FakeEquipmentDetailsViewModelFactory(),
                new FakeEquipmentFormModelService(),
                new FakeEquipmentMediaService(),
                new EquipmentWarrantyCsvExportService())
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            var result = await controller.WriteOff(15);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(EquipmentController.Details), redirect.ActionName);
            Assert.Equal(15, redirect.RouteValues!["id"]);
            Assert.Equal("Оборудование уже списано.", controller.TempData["ErrorMessage"]);
        }

        [Fact]
        public async Task InventoryCreate_Post_ShouldCreateSessionAndRedirectToDetails()
        {
            var inventoryService = new FakeInventoryService { CreatedSessionId = 9 };
            var userAccessService = new FakeUserAccessService(ModulePermission.CreateInventorySession);
            var controller = new InventoryController(
                inventoryService,
                userAccessService,
                new FakeInventoryLookupViewModelService(),
                new InventoryViewModelFactory(inventoryService, userAccessService))
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            var result = await controller.Create(new InventorySessionCreateViewModel
            {
                Name = "Апрельская инвентаризация",
                StartDate = new DateTime(2026, 4, 1)
            });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(InventoryController.Details), redirect.ActionName);
            Assert.Equal(9, redirect.RouteValues!["id"]);
            Assert.NotNull(inventoryService.LastCreateSessionDto);
            Assert.Equal("Апрельская инвентаризация", inventoryService.LastCreateSessionDto!.Name);
            Assert.Equal("TestUser", inventoryService.LastCreateSessionDto.CreatedBy);
            Assert.Equal("Сессия инвентаризации создана.", controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task InventoryCheck_Post_ShouldClearActualLocation_WhenItemNotFound()
        {
            var inventoryService = new FakeInventoryService();
            var userAccessService = new FakeUserAccessService(ModulePermission.CheckInventory);
            var controller = new InventoryController(
                inventoryService,
                userAccessService,
                new FakeInventoryLookupViewModelService(),
                new InventoryViewModelFactory(inventoryService, userAccessService))
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };

            var result = await controller.Check(new InventoryCheckViewModel
            {
                SessionId = 6,
                EquipmentId = 44,
                InventoryNumber = "INV-044",
                Name = "Ноутбук",
                ExpectedLocationName = "Кабинет 205",
                EquipmentStatusName = "В эксплуатации",
                IsFound = false,
                ActualLocationId = 3,
                ConditionComment = "На месте отсутствует"
            });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(InventoryController.Details), redirect.ActionName);
            Assert.Equal(6, redirect.RouteValues!["id"]);
            Assert.NotNull(inventoryService.LastInventoryCheckDto);
            Assert.False(inventoryService.LastInventoryCheckDto!.IsFound);
            Assert.Null(inventoryService.LastInventoryCheckDto.ActualLocationId);
            Assert.Equal("На месте отсутствует", inventoryService.LastInventoryCheckDto.ConditionComment);
            Assert.Equal("Результат проверки сохранен.", controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task InventoryCheck_Post_ShouldReturnViewAndRepopulateLocations_WhenModelInvalid()
        {
            var inventoryService = new FakeInventoryService();
            var userAccessService = new FakeUserAccessService(ModulePermission.CheckInventory);
            var lookupService = new FakeInventoryLookupViewModelService();
            var controller = new InventoryController(
                inventoryService,
                userAccessService,
                lookupService,
                new InventoryViewModelFactory(inventoryService, userAccessService))
            {
                ControllerContext = ControllerTestHelper.CreateControllerContext(),
                TempData = ControllerTestHelper.CreateTempData()
            };
            controller.ModelState.AddModelError(nameof(InventoryCheckViewModel.ConditionComment), "Ошибка проверки");

            var model = new InventoryCheckViewModel
            {
                SessionId = 2,
                EquipmentId = 8,
                InventoryNumber = "INV-008",
                Name = "Проектор",
                ExpectedLocationName = "Актовый зал",
                EquipmentStatusName = "В эксплуатации",
                IsFound = true
            };

            var result = await controller.Check(model);

            var view = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<InventoryCheckViewModel>(view.Model);
            Assert.Same(model, returnedModel);
            Assert.Equal(1, lookupService.PopulateLocationsCalls);
            Assert.Null(inventoryService.LastInventoryCheckDto);
        }

        private static void AttachForm(Controller controller, IFormCollection form)
        {
            controller.ControllerContext.HttpContext.Request.Form = form;
        }

        private static IFormCollection CreateEquipmentForm(string fileName)
        {
            var bytes = new byte[] { 1, 2, 3, 4 };
            var stream = new MemoryStream(bytes);
            IFormFile photo = new FormFile(stream, 0, bytes.Length, nameof(EquipmentCreateViewModel.Photo), fileName);

            return new FormCollection(
                new Dictionary<string, StringValues>
                {
                    [nameof(EquipmentCreateViewModel.InventoryNumber)] = "INV-500",
                    [nameof(EquipmentCreateViewModel.Name)] = "Рабочая станция преподавателя",
                    [nameof(EquipmentCreateViewModel.EquipmentTypeId)] = "1",
                    [nameof(EquipmentCreateViewModel.EquipmentStatusId)] = "1",
                    [nameof(EquipmentCreateViewModel.LocationId)] = "1",
                    [nameof(EquipmentCreateViewModel.SerialNumber)] = "SN-500",
                    [nameof(EquipmentCreateViewModel.Manufacturer)] = "Dell",
                    [nameof(EquipmentCreateViewModel.Model)] = "OptiPlex 7090",
                    [nameof(EquipmentCreateViewModel.PurchaseDate)] = "2026-03-30",
                    [nameof(EquipmentCreateViewModel.CommissioningDate)] = "2026-03-31",
                    [nameof(EquipmentCreateViewModel.WarrantyEndDate)] = "2027-03-31",
                    [nameof(EquipmentCreateViewModel.ResponsiblePerson)] = "Иванов И.И.",
                    [nameof(EquipmentCreateViewModel.Notes)] = "Основной кабинет",
                    [nameof(EquipmentCreateViewModel.RemovePhoto)] = "false"
                },
                new FormFileCollection { photo });
        }
    }
}
