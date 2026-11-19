using FarmacorpPOS.ConsoleApp.Utilidades;
using FarmacorpPOS.Services.DTOs.Estrategias;
using FarmacorpPOS.Services.Estrategias;

namespace FarmacorpPOS.ConsoleApp.Controllers.Estrategias;

public class EstrategiaController
{
    private readonly IEstrategiaService _estrategiaService;

    public EstrategiaController(IEstrategiaService estrategiaService)
    {
        _estrategiaService = estrategiaService;
    }

    public Task CambiarAsync()
    {
        Pantalla.Titulo("Cambiar estrategia de negocio");
        Pantalla.ComoVolver();

        var estrategias = _estrategiaService.Listar();
        for (var i = 0; i < estrategias.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {estrategias[i].Nombre}{(estrategias[i].Activa ? "  (activa)" : "")}");
        }

        Console.WriteLine();
        var opcion = Entrada.LeerEntero("Estrategia", minimo: 1, maximo: estrategias.Count);

        var activa = _estrategiaService.Cambiar(new CambiarEstrategiaSolicitudDTO(estrategias[opcion - 1].Nombre));

        Pantalla.Exito($"Estrategia activa: {activa.Nombre}. Se aplica desde la próxima operación.");

        // Se avisa porque el cambio no se guarda en ningún archivo: quien
        // reinicie la aplicación podría esperar seguir con esta estrategia.
        Pantalla.Aviso("Al reiniciar, la aplicación vuelve a la estrategia del appsettings.json.");

        return Task.CompletedTask;
    }
}
