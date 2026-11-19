using FarmacorpPOS.Domain.Entities.Erp;
using FarmacorpPOS.Domain.Entities.Express.Categorias;
using FarmacorpPOS.Domain.Entities.Express.Productos;
using FarmacorpPOS.Domain.Entities.Express.Ventas;
using FarmacorpPOS.Infrastructure.Data;

namespace FarmacorpPOS.Repositories.UnitsOfWork;

// No libera el DbContext: lo crea y lo cierra el contenedor de DI, y si lo
// liberáramos aquí otro servicio del mismo scope se quedaría sin él.
public class UnitOfWork : IUnitOfWork
{
    private readonly FarmacorpDbContext _context;

    // Se crean solo cuando alguien los pide; la mayoría de las transacciones
    // usa dos o tres repositorios, no los siete.
    private IRepository<TipoProducto>? _tiposProducto;
    private IRepository<ExpProducto>? _expProductos;
    private IRepository<Categoria>? _categorias;
    private IRepository<ProductoCategoria>? _productosCategorias;
    private IRepository<VentaExpress>? _ventasExpress;
    private IRepository<ErpProducto>? _erpProductos;
    private IRepository<CodigoBarra>? _codigosBarras;

    public UnitOfWork(FarmacorpDbContext context)
    {
        _context = context;
    }

    public IRepository<TipoProducto> TiposProducto => _tiposProducto ??= new Repository<TipoProducto>(_context);
    public IRepository<ExpProducto> ExpProductos => _expProductos ??= new Repository<ExpProducto>(_context);
    public IRepository<Categoria> Categorias => _categorias ??= new Repository<Categoria>(_context);
    public IRepository<ProductoCategoria> ProductosCategorias => _productosCategorias ??= new Repository<ProductoCategoria>(_context);
    public IRepository<VentaExpress> VentasExpress => _ventasExpress ??= new Repository<VentaExpress>(_context);
    public IRepository<ErpProducto> ErpProductos => _erpProductos ??= new Repository<ErpProducto>(_context);
    public IRepository<CodigoBarra> CodigosBarras => _codigosBarras ??= new Repository<CodigoBarra>(_context);

    public Task<int> GuardarCambiosAsync() => _context.SaveChangesAsync();
}
