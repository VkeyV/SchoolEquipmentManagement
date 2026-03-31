using Microsoft.Extensions.DependencyInjection;
using SchoolEquipmentManagement.Application.Interfaces;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;
using SchoolEquipmentManagement.Application.Services;
using SchoolEquipmentManagement.Infrastructure.Repositories;
using SchoolEquipmentManagement.Web.Security;
using SchoolEquipmentManagement.Web.Services.Equipment;
using SchoolEquipmentManagement.Web.Services.Inventory;

namespace SchoolEquipmentManagement.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebServices(this IServiceCollection services)
        {
            services.AddScoped<IEquipmentService, EquipmentService>();
            services.AddScoped<IEquipmentImportService, EquipmentImportService>();
            services.AddScoped<IEquipmentHistoryService, EquipmentHistoryService>();
            services.AddScoped<IDictionaryService, DictionaryService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IUserManagementService, UserManagementService>();
            services.AddScoped<ISecurityAuditService, SecurityAuditService>();
            services.AddScoped<IPasswordHashService, PasswordHashService>();
            services.AddScoped<IUserAccessService, UserAccessService>();
            services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<IEquipmentMediaService, EquipmentMediaService>();
            services.AddScoped<IEquipmentWarrantyCsvExportService, EquipmentWarrantyCsvExportService>();
            services.AddScoped<IEquipmentLookupViewModelService, EquipmentLookupViewModelService>();
            services.AddScoped<IEquipmentDetailsViewModelFactory, EquipmentDetailsViewModelFactory>();
            services.AddSingleton<IEquipmentFormModelService, EquipmentFormModelService>();
            services.AddScoped<IInventoryLookupViewModelService, InventoryLookupViewModelService>();
            services.AddScoped<IInventoryViewModelFactory, InventoryViewModelFactory>();

            return services;
        }
    }
}
