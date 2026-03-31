using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SchoolEquipmentManagement.Web.Security;

namespace SchoolEquipmentManagement.Web.Extensions
{
    public static class AuthorizationServiceCollectionExtensions
    {
        public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
        {
            services.AddScoped<IAuthorizationHandler, ModulePermissionAuthorizationHandler>();

            services.AddAuthorization(options =>
            {
                foreach (var permission in Enum.GetValues<ModulePermission>())
                {
                    options.AddPolicy(
                        PermissionPolicyNames.For(permission),
                        policy => policy.RequireAuthenticatedUser()
                            .AddRequirements(new ModulePermissionRequirement(permission)));
                }
            });

            return services;
        }
    }
}
