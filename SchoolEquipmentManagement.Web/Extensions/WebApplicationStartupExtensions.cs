using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;
using SchoolEquipmentManagement.Infrastructure;
using SchoolEquipmentManagement.Infrastructure.Data;
using SchoolEquipmentManagement.Infrastructure.Seed;
using SchoolEquipmentManagement.Web.Security;
using System.Globalization;

namespace SchoolEquipmentManagement.Web.Extensions
{
    public static class WebApplicationStartupExtensions
    {
        public static WebApplicationBuilder AddWebStartup(this WebApplicationBuilder builder)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var supportedCultures = new[]
            {
                new CultureInfo("ru-RU")
            };

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });

            builder.Services.AddPermissionAuthorization();
            builder.Services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.Filters.Add(new AuthorizeFilter(policy));
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
            builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("ru-RU");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddWebServices();

            return builder;
        }

        public static WebApplication UseWebStartup(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            var localizationOptions = app.Services
                .GetRequiredService<IOptions<RequestLocalizationOptions>>()
                .Value;

            app.UseRequestLocalization(localizationOptions);
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            return app;
        }

        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await DbSeeder.SeedAsync(context);
        }
    }
}
