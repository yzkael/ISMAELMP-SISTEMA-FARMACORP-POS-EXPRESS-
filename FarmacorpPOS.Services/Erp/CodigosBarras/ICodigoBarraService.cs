using FarmacorpPOS.Services.DTOs.Erp.CodigosBarras;

namespace FarmacorpPOS.Services.Erp.CodigosBarras;

public interface ICodigoBarraService
{
    Task<CodigoBarraAsignadoDTO> AsignarAsync(AsignarCodigoBarraSolicitudDTO solicitud);
}
