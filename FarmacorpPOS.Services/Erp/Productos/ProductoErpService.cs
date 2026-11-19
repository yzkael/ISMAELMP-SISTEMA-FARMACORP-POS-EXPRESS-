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

    // Se traen las tablas completas y se cruzan en memoria porque el
    // repositorio genérico no expone joins; para el volumen de un POS de
    // prueba es más simple que agregar consultas específicas.
    public async Task<IReadOnlyList<ProductoResumenDTO>> ListarAsync()
    {
        var productos = await _unitOfWork.ExpProductos.ObtenerTodosAsync();
        var tipos = (await _unitOfWork.TiposProducto.ObtenerTodosAsync()).ToDictionary(t => t.IdTipoProducto, t => t.Descripcion);
        var fichasErp = (await _unitOfWork.ErpProductos.ObtenerTodosAsync()).ToDictionary(e => e.IdProducto);
        var categoriasPorProducto = (await _unitOfWork.ProductosCategorias.ObtenerTodosAsync())
            .GroupBy(pc => pc.IdProducto)
            .ToDictionary(g => g.Key, g => g.Count());
        var codigosPorProducto = (await _unitOfWork.CodigosBarras.ObtenerTodosAsync())
            .GroupBy(c => c.IdProducto)
            .ToDictionary(g => g.Key, g => g.Count());

        return productos
            .OrderBy(p => p.IdProducto)
            .Select(p =>
            {
                fichasErp.TryGetValue(p.IdProducto, out var erp);
                return new ProductoResumenDTO(
                    p.IdProducto,
                    p.Nombre,
                    tipos.GetValueOrDefault(p.IdTipoProducto, "-"),
                    p.Precio,
                    p.Activo,
                    erp?.Stock,
                    erp?.UniqueCodigo,
                    categoriasPorProducto.GetValueOrDefault(p.IdProducto),
                    codigosPorProducto.GetValueOrDefault(p.IdProducto));
            })
            .ToList();
    }

    public async Task<IReadOnlyList<TipoProductoDTO>> ListarTiposAsync()
    {
        var tipos = await _unitOfWork.TiposProducto.ObtenerTodosAsync();

        return tipos
            .OrderBy(t => t.IdTipoProducto)
            .Select(t => new TipoProductoDTO(t.IdTipoProducto, t.Descripcion))
            .ToList();
    }
}
