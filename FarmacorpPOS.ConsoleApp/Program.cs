using System.Globalization;
using System.Text;
using FarmacorpPOS.ConsoleApp.Menus;
using FarmacorpPOS.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.OutputEncoding = Encoding.UTF8;

// Montos y fechas con el mismo formato en cualquier máquina, sin depender de
// su configuración regional.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // appsettings.json se copia junto al ejecutable; así se encuentra sin
    // importar desde qué carpeta se lance la aplicación.
    ContentRootPath = AppContext.BaseDirectory,
});

builder.AddInfrastructureServices();
builder.AddRepositoryServices();
builder.AddBusinessServices();
builder.AddPresentationServices();

using var host = builder.Build();

await host.SeedDatabaseAsync();
await host.Services.GetRequiredService<MenuPrincipal>().MostrarAsync();
