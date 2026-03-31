using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SchoolEquipmentManagement.Application.DTOs;
using SchoolEquipmentManagement.Web.ViewModels.Equipment;

namespace SchoolEquipmentManagement.Web.Services.Equipment;

public interface IEquipmentFormModelService
{
    EquipmentCreateViewModel HydrateCreateModel(EquipmentCreateViewModel model, IFormCollection form, ModelStateDictionary modelState);
    EquipmentEditViewModel HydrateEditModel(EquipmentEditViewModel model, IFormCollection form, ModelStateDictionary modelState);
    void ValidateEquipmentModel(EquipmentCreateViewModel model, ModelStateDictionary modelState);
    void ValidatePhoto(IFormFile? photo, ModelStateDictionary modelState);
    CreateEquipmentDto CreateCreateDto(EquipmentCreateViewModel model, string changedBy);
    UpdateEquipmentDto CreateUpdateDto(EquipmentEditViewModel model, string changedBy);
}
