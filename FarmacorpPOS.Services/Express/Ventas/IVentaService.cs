using FarmacorpPOS.Services.DTOs.Express.Ventas;

namespace FarmacorpPOS.Services.Express.Ventas;

public interface IVentaService
{
    Task<VentaRegistradaDTO> RegistrarAsync(RegistrarVentaSolicitudDTO solicitud);
}
