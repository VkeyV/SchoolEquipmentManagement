using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;
using System.Globalization;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public sealed class EquipmentFormModelService : IEquipmentFormModelService
{
    private static readonly CultureInfo RuCulture = new("ru-RU");

    public EquipmentCreateViewModel HydrateCreateModel(EquipmentCreateViewModel model, IFormCollection form, ModelStateDictionary modelState)
    {
        model.InventoryNumber = GetRequiredText(form, nameof(EquipmentCreateViewModel.InventoryNumber));
        model.Name = GetRequiredText(form, nameof(EquipmentCreateViewModel.Name));
        model.EquipmentTypeId = GetNullableInt(form, nameof(EquipmentCreateViewModel.EquipmentTypeId));
        model.EquipmentStatusId = GetNullableInt(form, nameof(EquipmentCreateViewModel.EquipmentStatusId));
        model.LocationId = GetNullableInt(form, nameof(EquipmentCreateViewModel.LocationId));
        model.SerialNumber = GetOptionalText(form, nameof(EquipmentCreateViewModel.SerialNumber));
        model.Manufacturer = GetOptionalText(form, nameof(EquipmentCreateViewModel.Manufacturer));
        model.Model = GetOptionalText(form, nameof(EquipmentCreateViewModel.Model));
        model.PurchaseDate = GetNullableDate(form, nameof(EquipmentCreateViewModel.PurchaseDate), "Дата покупки", modelState);
        model.CommissioningDate = GetNullableDate(form, nameof(EquipmentCreateViewModel.CommissioningDate), "Дата ввода в эксплуатацию", modelState);
        model.WarrantyEndDate = GetNullableDate(form, nameof(EquipmentCreateViewModel.WarrantyEndDate), "Дата окончания гарантии", modelState);
        model.ResponsiblePerson = GetOptionalText(form, nameof(EquipmentCreateViewModel.ResponsiblePerson));
        model.Notes = GetOptionalText(form, nameof(EquipmentCreateViewModel.Notes));
        model.Photo = form.Files.GetFile(nameof(EquipmentCreateViewModel.Photo));
        model.RemovePhoto = GetCheckboxValue(form, nameof(EquipmentCreateViewModel.RemovePhoto));
        return model;
    }

    public EquipmentEditViewModel HydrateEditModel(EquipmentEditViewModel model, IFormCollection form, ModelStateDictionary modelState)
    {
        model.Id = GetNullableInt(form, nameof(EquipmentEditViewModel.Id)) ?? model.Id;
        HydrateCreateModel(model, form, modelState);
        return model;
    }

    public void ValidateEquipmentModel(EquipmentCreateViewModel model, ModelStateDictionary modelState)
    {
        var existingErrors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(
                pair => pair.Value!.Errors.Select(error => new KeyValuePair<string, string>(pair.Key, error.ErrorMessage)))
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .Distinct()
            .ToList();

        modelState.Clear();

        foreach (var error in existingErrors)
        {
            modelState.AddModelError(error.Key, error.Value);
        }

        if (string.IsNullOrWhiteSpace(model.InventoryNumber))
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.InventoryNumber), "Укажите инвентарный номер.");
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.Name), "Укажите наименование оборудования.");
        }

        if (!model.EquipmentTypeId.HasValue)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.EquipmentTypeId), "Выберите тип оборудования.");
        }

        if (!model.EquipmentStatusId.HasValue)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.EquipmentStatusId), "Выберите статус.");
        }

        if (!model.LocationId.HasValue)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.LocationId), "Выберите местоположение.");
        }

        if (model.InventoryNumber.Length > 50)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.InventoryNumber), "Инвентарный номер не должен превышать 50 символов.");
        }

        if (model.Name.Length > 200)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.Name), "Наименование не должно превышать 200 символов.");
        }

        if (!string.IsNullOrEmpty(model.SerialNumber) && model.SerialNumber.Length > 100)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.SerialNumber), "Серийный номер не должен превышать 100 символов.");
        }

        if (!string.IsNullOrEmpty(model.Manufacturer) && model.Manufacturer.Length > 150)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.Manufacturer), "Производитель не должен превышать 150 символов.");
        }

        if (!string.IsNullOrEmpty(model.Model) && model.Model.Length > 150)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.Model), "Модель не должна превышать 150 символов.");
        }

        if (!string.IsNullOrEmpty(model.ResponsiblePerson) && model.ResponsiblePerson.Length > 150)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.ResponsiblePerson), "Ответственное лицо не должно превышать 150 символов.");
        }

        if (!string.IsNullOrEmpty(model.Notes) && model.Notes.Length > 1000)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.Notes), "Примечание не должно превышать 1000 символов.");
        }

        if (model.PurchaseDate.HasValue && model.CommissioningDate.HasValue &&
            model.CommissioningDate.Value.Date < model.PurchaseDate.Value.Date)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.CommissioningDate), "Дата ввода в эксплуатацию не может быть раньше даты покупки.");
        }

        if (model.CommissioningDate.HasValue && model.WarrantyEndDate.HasValue &&
            model.WarrantyEndDate.Value.Date < model.CommissioningDate.Value.Date)
        {
            AddErrorIfMissing(modelState, nameof(EquipmentCreateViewModel.WarrantyEndDate), "Дата окончания гарантии не может быть раньше даты ввода в эксплуатацию.");
        }
    }

    public void ValidatePhoto(IFormFile? photo, ModelStateDictionary modelState)
    {
        if (photo is null || photo.Length == 0)
        {
            return;
        }

        const long maxFileSize = 5 * 1024 * 1024;
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        if (!allowedExtensions.Contains(extension))
        {
            modelState.AddModelError(nameof(EquipmentCreateViewModel.Photo), "Загрузите фотографию в формате JPG, PNG или WEBP.");
        }

        if (photo.Length > maxFileSize)
        {
            modelState.AddModelError(nameof(EquipmentCreateViewModel.Photo), "Размер фотографии не должен превышать 5 МБ.");
        }
    }

    public CreateEquipmentDto CreateCreateDto(EquipmentCreateViewModel model, string changedBy)
    {
        return new CreateEquipmentDto
        {
            InventoryNumber = model.InventoryNumber,
            Name = model.Name,
            EquipmentTypeId = model.EquipmentTypeId!.Value,
            EquipmentStatusId = model.EquipmentStatusId!.Value,
            LocationId = model.LocationId!.Value,
            SerialNumber = model.SerialNumber,
            Manufacturer = model.Manufacturer,
            Model = model.Model,
            PurchaseDate = model.PurchaseDate,
            CommissioningDate = model.CommissioningDate,
            WarrantyEndDate = model.WarrantyEndDate,
            ResponsiblePerson = model.ResponsiblePerson,
            Notes = model.Notes,
            ChangedBy = changedBy
        };
    }

    public UpdateEquipmentDto CreateUpdateDto(EquipmentEditViewModel model, string changedBy)
    {
        return new UpdateEquipmentDto
        {
            Id = model.Id,
            InventoryNumber = model.InventoryNumber,
            Name = model.Name,
            EquipmentTypeId = model.EquipmentTypeId!.Value,
            EquipmentStatusId = model.EquipmentStatusId!.Value,
            LocationId = model.LocationId!.Value,
            SerialNumber = model.SerialNumber,
            Manufacturer = model.Manufacturer,
            Model = model.Model,
            PurchaseDate = model.PurchaseDate,
            CommissioningDate = model.CommissioningDate,
            WarrantyEndDate = model.WarrantyEndDate,
            ResponsiblePerson = model.ResponsiblePerson,
            Notes = model.Notes,
            ChangedBy = changedBy
        };
    }

    private static string GetRequiredText(IFormCollection form, string key) =>
        form[key].ToString().Trim();

    private static string? GetOptionalText(IFormCollection form, string key)
    {
        var value = form[key].ToString().Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int? GetNullableInt(IFormCollection form, string key)
    {
        var rawValue = form[key].ToString().Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        return int.TryParse(rawValue, out var value) && value > 0 ? value : null;
    }

    private static bool GetCheckboxValue(IFormCollection form, string key)
    {
        var rawValue = form[key].ToString();
        return string.Equals(rawValue, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(rawValue, "on", StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime? GetNullableDate(IFormCollection form, string key, string displayName, ModelStateDictionary modelState)
    {
        var rawValue = form[key].ToString().Trim();
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        if (DateTime.TryParse(rawValue, RuCulture, DateTimeStyles.None, out var parsedRu))
        {
            return parsedRu.Date;
        }

        if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedInvariant))
        {
            return parsedInvariant.Date;
        }

        modelState.AddModelError(key, $"Укажите корректную дату в поле «{displayName}».");
        return null;
    }

    private static void AddErrorIfMissing(ModelStateDictionary modelState, string key, string message)
    {
        var errors = modelState.TryGetValue(key, out var entry)
            ? entry.Errors.Select(error => error.ErrorMessage)
            : Enumerable.Empty<string>();

        if (!errors.Contains(message, StringComparer.Ordinal))
        {
            modelState.AddModelError(key, message);
        }
    }
}
