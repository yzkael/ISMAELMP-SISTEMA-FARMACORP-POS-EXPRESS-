using FarmacorpPOS.Services.Erp.CodigosBarras;
using FarmacorpPOS.Services.Erp.Productos;
using FarmacorpPOS.Services.Estrategias;
using FarmacorpPOS.Services.Express.Categorias;
using FarmacorpPOS.Services.Express.Ventas;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddBusinessServices(this IHostApplicationBuilder builder)
    {
        AddEstrategia(builder);

        builder.Services.AddScoped<IProductoErpService, ProductoErpService>();
        builder.Services.AddScoped<ICodigoBarraService, CodigoBarraService>();
        builder.Services.AddScoped<ICategoriaService, CategoriaService>();
        builder.Services.AddScoped<IVentaService, VentaService>();
    }

    // Este es el único lugar que sabe qué estrategias existen. Se crean aquí y
    // no por DI porque no tienen dependencias ni estado: son solo reglas.
    private static void AddEstrategia(IHostApplicationBuilder builder)
    {
        var disponibles = new IEstrategiaNegocio[] { new EstrategiaBase(), new EstrategiaGanaMax() };

        // El appsettings.json solo dice con qué estrategia arranca la aplicación;
        // después se cambia desde el menú. Si no se configura nada, se usa Base
        // porque es la regla normal del negocio.
        var inicial = builder.Configuration["Estrategia"] ?? "Base";

        // Se crea ya, y no cuando alguien lo pida, para que un valor desconocido
        // impida arrancar en vez de fallar en la primera venta.
        var estrategiaService = new EstrategiaService(disponibles, inicial);
        builder.Services.AddSingleton<IEstrategiaService>(estrategiaService);

        // Scoped: cada operación del menú toma la estrategia activa al empezar.
        // Un cambio no la alcanza a mitad de camino y vale desde la siguiente.
        builder.Services.AddScoped<IEstrategiaNegocio>(_ => estrategiaService.Actual);
    }
}
