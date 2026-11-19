using FarmacorpPOS.Domain.Entities.Erp;
using FarmacorpPOS.Domain.Entities.Express.Productos;
using FarmacorpPOS.Repositories.UnitsOfWork;
using FarmacorpPOS.Services.DTOs.Erp.Productos;
using FarmacorpPOS.Services.Estrategias;
using FarmacorpPOS.Services.Excepciones;

namespace FarmacorpPOS.Services.Erp.Productos;

public class ProductoErpService : IProductoErpService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEstrategiaNegocio _estrategia;

    public ProductoErpService(IUnitOfWork unitOfWork, IEstrategiaNegocio estrategia)
    {
        _unitOfWork = unitOfWork;
        _estrategia = estrategia;
    }

    public async Task<ProductoErpRegistradoDTO> RegistrarAsync(RegistrarProductoErpSolicitudDTO solicitud)
    {
        if (string.IsNullOrWhiteSpace(solicitud.Nombre))
        {
            throw new ReglaNegocioException("El nombre del producto es obligatorio.");
        }

        if (solicitud.Costo <= 0)
        {
            throw new ReglaNegocioException("El costo debe ser mayor a cero.");
        }

        if (solicitud.Stock < 0)
        {
            throw new ReglaNegocioException("El stock inicial no puede ser negativo.");
        }

        if (!await _unitOfWork.TiposProducto.ExisteAsync(t => t.IdTipoProducto == solicitud.IdTipoProducto))
        {
            throw new ReglaNegocioException($"No existe el tipo de producto {solicitud.IdTipoProducto}.");
        }

        var producto = new ExpProducto
        {
            Nombre = solicitud.Nombre.Trim(),
            Precio = _estrategia.CalcularPrecio(solicitud.Costo),
            Activo = true,
            FechaVencimiento = solicitud.FechaVencimiento,
            Observaciones = solicitud.Observaciones,
            IdTipoProducto = solicitud.IdTipoProducto,

            // Se crea junto con el producto para que ambos se guarden en el mismo
            // GuardarCambiosAsync(): no queremos productos ERP a medio registrar.
            ErpProducto = new ErpProducto
            {
                Costo = solicitud.Costo,
                UniqueCodigo = Guid.NewGuid(),
                FechaRegistro = DateTime.Now,
                Stock = solicitud.Stock,
            },
        };

        _unitOfWork.ExpProductos.Agregar(producto);
        await _unitOfWork.GuardarCambiosAsync();

        return new ProductoErpRegistradoDTO(
            producto.IdProducto,
            producto.Nombre,
            producto.ErpProducto.Costo,
            producto.Precio,
            producto.ErpProducto.UniqueCodigo,
            producto.ErpProducto.Stock,
            _estrategia.Nombre);
    }
}
