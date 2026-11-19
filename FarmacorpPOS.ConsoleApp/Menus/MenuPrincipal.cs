using FarmacorpPOS.ConsoleApp.Controllers.Erp.CodigosBarras;
using FarmacorpPOS.ConsoleApp.Controllers.Erp.Productos;
using FarmacorpPOS.ConsoleApp.Controllers.Express.Categorias;
using FarmacorpPOS.ConsoleApp.Controllers.Express.Ventas;
using FarmacorpPOS.ConsoleApp.Utilidades;
using FarmacorpPOS.Services.Estrategias;
using FarmacorpPOS.Services.Excepciones;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FarmacorpPOS.ConsoleApp.Menus;

public class MenuPrincipal
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEstrategiaNegocio _estrategia;
    private readonly ILogger<MenuPrincipal> _logger;

    public MenuPrincipal(IServiceScopeFactory scopeFactory, IEstrategiaNegocio estrategia, ILogger<MenuPrincipal> logger)
    {
        _scopeFactory = scopeFactory;
        _estrategia = estrategia;
        _logger = logger;
    }

    public async Task MostrarAsync()
    {
        while (true)
        {
            MostrarOpciones();

            string opcion;
            try
            {
                opcion = Entrada.LeerTexto("Seleccione una opción");
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (VolverAlMenuException)
            {
                // Ya está en el menú: solo se vuelve a mostrar.
                continue;
            }

            if (opcion == "0")
            {
                Console.WriteLine("Hasta luego.");
                return;
            }

            Func<IServiceProvider, Task>? accion = opcion switch
            {
                "1" => sp => sp.GetRequiredService<ProductoErpController>().RegistrarAsync(),
                "2" => sp => sp.GetRequiredService<CodigoBarraController>().AsignarAsync(),
                "3" => sp => sp.GetRequiredService<CategoriaController>().AsignarAsync(),
                "4" => sp => sp.GetRequiredService<VentaController>().RegistrarAsync(),
                "5" => sp => sp.GetRequiredService<ProductoErpController>().ListarAsync(),
                _ => null,
            };

            if (accion is null)
            {
                Pantalla.Error("Opción no válida.");
                continue;
            }

            if (!await EjecutarAsync(accion))
            {
                return;
            }
        }
    }

    // Cada opción corre en su propio scope: tiene su propio DbContext, así lo
    // que quedó a medias en una operación fallida no se arrastra a la siguiente.
    // Los errores se manejan aquí una sola vez, en lugar de repetir el mismo
    // try/catch en cada Controller.
    private async Task<bool> EjecutarAsync(Func<IServiceProvider, Task> accion)
    {
        using var scope = _scopeFactory.CreateScope();

        try
        {
            await accion(scope.ServiceProvider);
        }
        catch (ReglaNegocioException ex)
        {
            Pantalla.Error(ex.Message);
        }
        catch (VolverAlMenuException)
        {
            Pantalla.Aviso("Operación cancelada. No se guardó nada.");
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al ejecutar la opción del menú.");
            Pantalla.Error("Ocurrió un error inesperado. El detalle quedó registrado arriba.");
        }

        return true;
    }

    private void MostrarOpciones()
    {
        Console.WriteLine();
        Console.WriteLine("=========================================");
        Console.WriteLine("        FARMACORP POS EXPRESS");
        Console.WriteLine($"        Estrategia activa: {_estrategia.Nombre}");
        Console.WriteLine("=========================================");
        Console.WriteLine(" 1. Registrar nuevo producto ERP");
        Console.WriteLine(" 2. Asignar código de barras a producto");
        Console.WriteLine(" 3. Asignar categoría a producto");
        Console.WriteLine(" 4. Registrar venta");
        Console.WriteLine(" 5. Listar productos");
        Console.WriteLine(" 0. Salir");
        Console.WriteLine("=========================================");
    }
}
