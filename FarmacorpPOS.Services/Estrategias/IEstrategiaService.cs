using FarmacorpPOS.Services.DTOs.Estrategias;

namespace FarmacorpPOS.Services.Estrategias;

public interface IEstrategiaService
{
    EstrategiaDTO ObtenerActiva();
    IReadOnlyList<EstrategiaDTO> Listar();
    EstrategiaDTO Cambiar(CambiarEstrategiaSolicitudDTO solicitud);
}
