using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolEquipmentManagement.Infrastructure.Data;
using SchoolEquipmentManagement.Infrastructure.Repositories;
using SchoolEquipmentManagement.Application.Interfaces.Repositories;


namespace SchoolEquipmentManagement.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IEquipmentRepository, EquipmentRepository>();
            services.AddScoped<IEquipmentHistoryRepository, EquipmentHistoryRepository>();
            services.AddScoped<IDictionaryRepository, DictionaryRepository>();
            services.AddScoped<IInventorySessionRepository, InventorySessionRepository>();
            services.AddScoped<IInventoryRecordRepository, InventoryRecordRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
