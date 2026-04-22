using LowCortisol.Application.Services;
using LowCortisol.Domain.Interfaces;
using LowCortisol.Infrastructure.Repositories;
using LowCortisol.Web.Components;
using LowCortisol.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<AppStateService>();
builder.Services.AddScoped<I18nService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();