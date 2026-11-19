using FarmacorpPOS.ConsoleApp.Controllers.Erp.CodigosBarras;
using FarmacorpPOS.ConsoleApp.Controllers.Erp.Productos;
using FarmacorpPOS.ConsoleApp.Controllers.Express.Categorias;
using FarmacorpPOS.ConsoleApp.Controllers.Express.Ventas;
using FarmacorpPOS.ConsoleApp.Menus;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        // Scoped igual que los servicios que usan: el menú crea un scope por opción.
        builder.Services.AddScoped<ProductoErpController>();
        builder.Services.AddScoped<CodigoBarraController>();
        builder.Services.AddScoped<CategoriaController>();
        builder.Services.AddScoped<VentaController>();

        builder.Services.AddSingleton<MenuPrincipal>();
    }
}
