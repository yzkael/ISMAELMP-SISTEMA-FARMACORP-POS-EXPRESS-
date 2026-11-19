using FarmacorpPOS.Services.DTOs.Estrategias;
using FarmacorpPOS.Services.Excepciones;

namespace FarmacorpPOS.Services.Estrategias;

// Guarda la estrategia con la que se está vendiendo. Es una sola instancia
// para toda la aplicación, así un cambio hecho desde el menú vale para todas
// las operaciones que vengan después, sin reiniciar.
// El cambio vive en memoria: al reiniciar, la aplicación vuelve a arrancar con
// la estrategia del appsettings.json.
public class EstrategiaService : IEstrategiaService
{
    private readonly IReadOnlyList<IEstrategiaNegocio> _disponibles;
    private IEstrategiaNegocio _actual;

    public EstrategiaService(IReadOnlyList<IEstrategiaNegocio> disponibles, string nombreInicial)
    {
        _disponibles = disponibles;

        // Mejor no arrancar que vender con una regla que nadie eligió.
        _actual = Buscar(nombreInicial)
            ?? throw new InvalidOperationException(
                $"Estrategia '{nombreInicial}' no reconocida en appsettings.json. Usa {NombresDisponibles()}.");
    }

    // Solo lo usa AddBusinessServices() para entregarle la estrategia activa a
    // los servicios; la consola trabaja únicamente con los DTOs.
    internal IEstrategiaNegocio Actual => _actual;

    public EstrategiaDTO ObtenerActiva() => new(_actual.Nombre, true);

    public IReadOnlyList<EstrategiaDTO> Listar() =>
        _disponibles.Select(e => new EstrategiaDTO(e.Nombre, e == _actual)).ToList();

    public EstrategiaDTO Cambiar(CambiarEstrategiaSolicitudDTO solicitud)
    {
        _actual = Buscar(solicitud.Nombre)
            ?? throw new ReglaNegocioException(
                $"No existe la estrategia '{solicitud.Nombre}'. Usa {NombresDisponibles()}.");

        return ObtenerActiva();
    }

    private IEstrategiaNegocio? Buscar(string nombre) =>
        _disponibles.FirstOrDefault(e => e.Nombre.Equals(nombre.Trim(), StringComparison.OrdinalIgnoreCase));

    private string NombresDisponibles() =>
        string.Join(" o ", _disponibles.Select(e => $"'{e.Nombre}'"));
}
