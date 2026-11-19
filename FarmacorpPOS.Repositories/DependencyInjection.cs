using FarmacorpPOS.Repositories.UnitsOfWork;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddRepositoryServices(this IHostApplicationBuilder builder)
    {
        // Scoped para que todos los servicios de una misma operación compartan
        // el mismo UnitOfWork y, con él, el mismo DbContext.
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
