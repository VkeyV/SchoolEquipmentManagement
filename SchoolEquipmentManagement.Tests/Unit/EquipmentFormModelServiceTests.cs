using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using SchoolEquipmentManagement.Web.Services.Equipment;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Tests.Unit
{
    public class EquipmentFormModelServiceTests
    {
        private readonly EquipmentFormModelService _service = new();

        [Fact]
        public void HydrateCreateModel_ShouldTrimParseAndAttachPhoto()
        {
            using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var photo = new FormFile(stream, 0, stream.Length, nameof(EquipmentCreateViewModel.Photo), "device.png");
            var form = new FormCollection(
                new Dictionary<string, StringValues>
                {
                    [nameof(EquipmentCreateViewModel.InventoryNumber)] = " INV-123 ",
                    [nameof(EquipmentCreateViewModel.Name)] = " Рабочая станция ",
                    [nameof(EquipmentCreateViewModel.EquipmentTypeId)] = "2",
                    [nameof(EquipmentCreateViewModel.EquipmentStatusId)] = "3",
                    [nameof(EquipmentCreateViewModel.LocationId)] = "4",
                    [nameof(EquipmentCreateViewModel.SerialNumber)] = " SN-01 ",
                    [nameof(EquipmentCreateViewModel.Manufacturer)] = " Dell ",
                    [nameof(EquipmentCreateViewModel.Model)] = " OptiPlex ",
                    [nameof(EquipmentCreateViewModel.PurchaseDate)] = "30.03.2026",
                    [nameof(EquipmentCreateViewModel.CommissioningDate)] = "2026-03-31",
                    [nameof(EquipmentCreateViewModel.WarrantyEndDate)] = "01.04.2027",
                    [nameof(EquipmentCreateViewModel.ResponsiblePerson)] = " Иванов И.И. ",
                    [nameof(EquipmentCreateViewModel.Notes)] = " Основной кабинет ",
                    [nameof(EquipmentCreateViewModel.RemovePhoto)] = "on"
                },
                new FormFileCollection { photo });
            var modelState = new ModelStateDictionary();

            var model = _service.HydrateCreateModel(new EquipmentCreateViewModel(), form, modelState);

            Assert.Equal("INV-123", model.InventoryNumber);
            Assert.Equal("Рабочая станция", model.Name);
            Assert.Equal(2, model.EquipmentTypeId);
            Assert.Equal(3, model.EquipmentStatusId);
            Assert.Equal(4, model.LocationId);
            Assert.Equal("SN-01", model.SerialNumber);
            Assert.Equal("Dell", model.Manufacturer);
            Assert.Equal("OptiPlex", model.Model);
            Assert.Equal(new DateTime(2026, 3, 30), model.PurchaseDate);
            Assert.Equal(new DateTime(2026, 3, 31), model.CommissioningDate);
            Assert.Equal(new DateTime(2027, 4, 1), model.WarrantyEndDate);
            Assert.Equal("Иванов И.И.", model.ResponsiblePerson);
            Assert.Equal("Основной кабинет", model.Notes);
            Assert.Same(photo, model.Photo);
            Assert.True(model.RemovePhoto);
            Assert.True(modelState.IsValid);
        }

        [Fact]
        public void HydrateEditModel_ShouldPreserveExistingId_WhenFormDoesNotContainIt()
        {
            var form = new FormCollection(
                new Dictionary<string, StringValues>
                {
                    [nameof(EquipmentCreateViewModel.InventoryNumber)] = "INV-321",
                    [nameof(EquipmentCreateViewModel.Name)] = "Монитор"
                });

            var model = _service.HydrateEditModel(new EquipmentEditViewModel { Id = 42 }, form, new ModelStateDictionary());

            Assert.Equal(42, model.Id);
            Assert.Equal("INV-321", model.InventoryNumber);
            Assert.Equal("Монитор", model.Name);
        }

        [Fact]
        public void ValidateEquipmentModel_ShouldPreserveExistingErrorsAndAddDomainValidation()
        {
            var model = new EquipmentCreateViewModel
            {
                PurchaseDate = new DateTime(2026, 4, 10),
                CommissioningDate = new DateTime(2026, 4, 9)
            };
            var modelState = new ModelStateDictionary();
            modelState.AddModelError(nameof(EquipmentCreateViewModel.PurchaseDate), "Укажите корректную дату в поле «Дата покупки».");

            _service.ValidateEquipmentModel(model, modelState);

            Assert.Contains(modelState[nameof(EquipmentCreateViewModel.PurchaseDate)]!.Errors, x => x.ErrorMessage == "Укажите корректную дату в поле «Дата покупки».");
            Assert.Contains(modelState[nameof(EquipmentCreateViewModel.InventoryNumber)]!.Errors, x => x.ErrorMessage == "Укажите инвентарный номер.");
            Assert.Contains(modelState[nameof(EquipmentCreateViewModel.Name)]!.Errors, x => x.ErrorMessage == "Укажите наименование оборудования.");
            Assert.Contains(modelState[nameof(EquipmentCreateViewModel.EquipmentTypeId)]!.Errors, x => x.ErrorMessage == "Выберите тип оборудования.");
            Assert.Contains(modelState[nameof(EquipmentCreateViewModel.EquipmentStatusId)]!.Errors, x => x.ErrorMessage == "Выберите статус.");
            Assert.Contains(modelState[nameof(EquipmentCreateViewModel.LocationId)]!.Errors, x => x.ErrorMessage == "Выберите местоположение.");
            Assert.Contains(modelState[nameof(EquipmentCreateViewModel.CommissioningDate)]!.Errors, x => x.ErrorMessage == "Дата ввода в эксплуатацию не может быть раньше даты покупки.");
        }

        [Fact]
        public void ValidatePhoto_ShouldAddErrors_ForInvalidExtensionAndOversize()
        {
            using var stream = new MemoryStream(new byte[16]);
            IFormFile photo = new FormFile(stream, 0, 6 * 1024 * 1024, nameof(EquipmentCreateViewModel.Photo), "device.gif");
            var modelState = new ModelStateDictionary();

            _service.ValidatePhoto(photo, modelState);

            var errors = modelState[nameof(EquipmentCreateViewModel.Photo)]!.Errors.Select(x => x.ErrorMessage).ToList();
            Assert.Contains("Загрузите фотографию в формате JPG, PNG или WEBP.", errors);
            Assert.Contains("Размер фотографии не должен превышать 5 МБ.", errors);
        }

        [Fact]
        public void CreateDtos_ShouldMapViewModelFields()
        {
            var createModel = new EquipmentCreateViewModel
            {
                InventoryNumber = "INV-900",
                Name = "Принтер",
                EquipmentTypeId = 2,
                EquipmentStatusId = 3,
                LocationId = 4,
                SerialNumber = "SN-900",
                Manufacturer = "HP",
                Model = "LaserJet",
                PurchaseDate = new DateTime(2026, 1, 10),
                CommissioningDate = new DateTime(2026, 1, 12),
                WarrantyEndDate = new DateTime(2027, 1, 12),
                ResponsiblePerson = "Петров П.П.",
                Notes = "Кабинет информатики"
            };

            var createDto = _service.CreateCreateDto(createModel, "Tester");
            var updateDto = _service.CreateUpdateDto(new EquipmentEditViewModel
            {
                Id = 55,
                InventoryNumber = createModel.InventoryNumber,
                Name = createModel.Name,
                EquipmentTypeId = createModel.EquipmentTypeId,
                EquipmentStatusId = createModel.EquipmentStatusId,
                LocationId = createModel.LocationId,
                SerialNumber = createModel.SerialNumber,
                Manufacturer = createModel.Manufacturer,
                Model = createModel.Model,
                PurchaseDate = createModel.PurchaseDate,
                CommissioningDate = createModel.CommissioningDate,
                WarrantyEndDate = createModel.WarrantyEndDate,
                ResponsiblePerson = createModel.ResponsiblePerson,
                Notes = createModel.Notes
            }, "Editor");

            Assert.Equal("INV-900", createDto.InventoryNumber);
            Assert.Equal("Tester", createDto.ChangedBy);
            Assert.Equal(55, updateDto.Id);
            Assert.Equal("Editor", updateDto.ChangedBy);
            Assert.Equal("LaserJet", updateDto.Model);
        }
    }
}
