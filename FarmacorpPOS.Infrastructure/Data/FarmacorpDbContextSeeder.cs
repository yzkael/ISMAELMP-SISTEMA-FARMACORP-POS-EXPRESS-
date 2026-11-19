using FarmacorpPOS.Domain.Entities.Express.Categorias;
using FarmacorpPOS.Domain.Entities.Express.Productos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FarmacorpPOS.Infrastructure.Data;

public static class SeederExtensions
{
    // Se llama una vez al arrancar la consola. Así quien clone el repo solo
    // tiene que ejecutar la app: la base se crea, se migra y queda con los
    // datos mínimos para probar las transacciones.
    public static async Task SeedDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();

        var seeder = scope.ServiceProvider.GetRequiredService<FarmacorpDbContextSeeder>();

        await seeder.MigrateAsync();
        await seeder.SeedAsync();
    }
}

public class FarmacorpDbContextSeeder
{
    private readonly ILogger<FarmacorpDbContextSeeder> _logger;
    private readonly FarmacorpDbContext _context;

    public FarmacorpDbContextSeeder(ILogger<FarmacorpDbContextSeeder> logger, FarmacorpDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task MigrateAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al migrar la base de datos.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error al cargar los datos iniciales.");
            throw;
        }
    }

    // Cada dato se busca por su descripción antes de insertarlo, así se puede
    // correr en cada arranque sin duplicar nada, y si mañana se agrega un tipo
    // o una categoría nueva a esta lista, solo se inserta lo que falta.
    public async Task TrySeedAsync()
    {
        var tiposProducto = new[] { "Medicamento", "Cuidado personal", "Suplemento" };

        var subcategoriasDeMedicamentos = new[] { "Analgésicos", "Antigripales" };

        var otrasCategorias = new[] { "Vitaminas", "Higiene" };


        foreach (var tipo in tiposProducto)
        {
            await AgregarTipoProductoSiNoExisteAsync(tipo);
        }

        // Los padres se guardan primero para que las subcategorías puedan
        // apuntar a ellos sin depender de Ids fijos.
        var medicamentos = await AgregarCategoriaSiNoExisteAsync("Medicamentos");

        foreach (var categoria in otrasCategorias)
        {
            await AgregarCategoriaSiNoExisteAsync(categoria);
        }

        foreach (var subcategoria in subcategoriasDeMedicamentos)
        {
            await AgregarCategoriaSiNoExisteAsync(subcategoria, medicamentos);
        }

        await _context.SaveChangesAsync();
    }

    private async Task AgregarTipoProductoSiNoExisteAsync(string descripcion)
    {
        if (await _context.TiposProducto.AnyAsync(t => t.Descripcion == descripcion))
        {
            return;
        }

        _context.TiposProducto.Add(new TipoProducto { Descripcion = descripcion });
    }

    private async Task<Categoria> AgregarCategoriaSiNoExisteAsync(string descripcion, Categoria? padre = null)
    {
        var existente = await _context.Categorias.FirstOrDefaultAsync(c => c.Descripcion == descripcion);
        if (existente is not null)
        {
            return existente;
        }

        var categoria = new Categoria { Descripcion = descripcion, Activo = true, CategoriaPadre = padre };
        _context.Categorias.Add(categoria);
        return categoria;
    }
}
