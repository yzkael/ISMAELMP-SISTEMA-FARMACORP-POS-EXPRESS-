using FarmacorpPOS.Domain.Entities.Erp;
using FarmacorpPOS.Domain.Entities.Express.Categorias;
using FarmacorpPOS.Domain.Entities.Express.Productos;
using FarmacorpPOS.Domain.Entities.Express.Ventas;
using Microsoft.EntityFrameworkCore;

namespace FarmacorpPOS.Infrastructure.Data;

public class FarmacorpDbContext : DbContext
{
    public FarmacorpDbContext(DbContextOptions<FarmacorpDbContext> options) : base(options)
    {
    }

    public DbSet<TipoProducto> TiposProducto => Set<TipoProducto>();
    public DbSet<ExpProducto> ExpProductos => Set<ExpProducto>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<ProductoCategoria> ProductosCategorias => Set<ProductoCategoria>();
    public DbSet<VentaExpress> VentasExpress => Set<VentaExpress>();
    public DbSet<ErpProducto> ErpProductos => Set<ErpProducto>();
    public DbSet<CodigoBarra> CodigosBarras => Set<CodigoBarra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Toma todas las clases de Configurations/ automáticamente, así una
        // entidad nueva solo necesita su configuración y no hay que acordarse
        // de registrarla aquí.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FarmacorpDbContext).Assembly);
    }
}
