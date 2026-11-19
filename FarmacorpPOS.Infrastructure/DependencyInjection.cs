using FarmacorpPOS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// Va en el namespace de DependencyInjection para que Program.cs pueda llamar a
// builder.AddInfrastructureServices() sin agregar un using por cada capa.
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        // Mejor fallar al arrancar con un mensaje claro que en la primera
        // consulta con un error de conexión difícil de entender.
        var connectionString = builder.Configuration.GetConnectionString("FarmacorpDb")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'FarmacorpDb' en appsettings.json.");

        builder.Services.AddDbContext<FarmacorpDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddScoped<FarmacorpDbContextSeeder>();
    }
}
