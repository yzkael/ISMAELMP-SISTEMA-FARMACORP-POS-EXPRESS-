using FarmacorpPOS.Domain.Entities.Erp;
using FarmacorpPOS.Repositories.UnitsOfWork;
using FarmacorpPOS.Services.DTOs.Erp.CodigosBarras;
using FarmacorpPOS.Services.Excepciones;

namespace FarmacorpPOS.Services.Erp.CodigosBarras;

public class CodigoBarraService : ICodigoBarraService
{
    private readonly IUnitOfWork _unitOfWork;

    public CodigoBarraService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CodigoBarraAsignadoDTO> AsignarAsync(AsignarCodigoBarraSolicitudDTO solicitud)
    {
        var producto = await _unitOfWork.ExpProductos.ObtenerPorIdAsync(solicitud.IdProducto)
            ?? throw new ReglaNegocioException($"No existe el producto {solicitud.IdProducto}.");

        var codigo = new CodigoBarra
        {
            UniqueCodigo = Guid.NewGuid(),
            Activo = true,
            IdProducto = producto.IdProducto,
        };

        _unitOfWork.CodigosBarras.Agregar(codigo);
        await _unitOfWork.GuardarCambiosAsync();

        return new CodigoBarraAsignadoDTO(codigo.IdCodigoBarra, producto.IdProducto, producto.Nombre, codigo.UniqueCodigo);
    }
}
