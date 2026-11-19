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

    // Este es el único lugar que sabe qué estrategias existen. Para volver a
    // Base al terminar GanaMax basta con cambiar "Estrategia" en el
    // appsettings.json y reiniciar.
    private static void AddEstrategia(IHostApplicationBuilder builder)
    {
        // Si no se configura nada, se usa Base porque es la regla normal del negocio.
        var estrategia = builder.Configuration["Estrategia"] ?? "Base";

        if (estrategia.Equals("Base", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddSingleton<IEstrategiaNegocio, EstrategiaBase>();
        }
        else if (estrategia.Equals("GanaMax", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.AddSingleton<IEstrategiaNegocio, EstrategiaGanaMax>();
        }
        else
        {
            // Mejor no arrancar que vender con una regla que nadie eligió.
            throw new InvalidOperationException(
                $"Estrategia '{estrategia}' no reconocida en appsettings.json. Usa 'Base' o 'GanaMax'.");
        }
    }
}
