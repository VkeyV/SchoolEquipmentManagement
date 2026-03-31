using SchoolEquipmentManagement.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddWebStartup();

var app = builder.Build();
app.UseWebStartup();
await app.SeedDatabaseAsync();

app.Run();
