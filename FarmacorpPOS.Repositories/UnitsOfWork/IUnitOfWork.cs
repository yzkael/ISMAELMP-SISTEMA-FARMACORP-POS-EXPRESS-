using FarmacorpPOS.Domain.Entities.Erp;
using FarmacorpPOS.Domain.Entities.Express.Categorias;
using FarmacorpPOS.Domain.Entities.Express.Productos;
using FarmacorpPOS.Domain.Entities.Express.Ventas;

namespace FarmacorpPOS.Repositories.UnitsOfWork;

// Todos los repositorios comparten el mismo DbContext, así lo que hagan
// varios de ellos en una transacción de negocio se confirma en un solo
// GuardarCambiosAsync(): todo o nada.
public interface IUnitOfWork
{
    IRepository<TipoProducto> TiposProducto { get; }
    IRepository<ExpProducto> ExpProductos { get; }
    IRepository<Categoria> Categorias { get; }
    IRepository<ProductoCategoria> ProductosCategorias { get; }
    IRepository<VentaExpress> VentasExpress { get; }
    IRepository<ErpProducto> ErpProductos { get; }
    IRepository<CodigoBarra> CodigosBarras { get; }

    Task<int> GuardarCambiosAsync();
}
