using FarmacorpPOS.Services.DTOs.Erp.Productos;

namespace FarmacorpPOS.Services.Erp.Productos;

public interface IProductoErpService
{
    Task<ProductoErpRegistradoDTO> RegistrarAsync(RegistrarProductoErpSolicitudDTO solicitud);
}
