using FarmacorpPOS.Domain.Entities.Express.Categorias;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacorpPOS.Infrastructure.Data.Configurations.Express.Categorias;

public class ProductoCategoriaConfiguration : IEntityTypeConfiguration<ProductoCategoria>
{
    public void Configure(EntityTypeBuilder<ProductoCategoria> builder)
    {
        builder.ToTable("ProductosCategorias");
        builder.HasKey(pc => pc.IdDetalle);

        // Si se pudiera asignar dos veces la misma categoría, un producto con una
        // sola categoría real contaría como dos y perdería el descuento de la Regla 3.
        builder.HasIndex(pc => new { pc.IdProducto, pc.IdCategoria }).IsUnique();

        builder.HasOne(pc => pc.Producto)
            .WithMany(p => p.ProductosCategorias)
            .HasForeignKey(pc => pc.IdProducto)
            .OnDelete(DeleteBehavior.Cascade);

        // Una categoría con productos asignados no se debería poder borrar sin
        // antes decidir qué pasa con esos productos.
        builder.HasOne(pc => pc.Categoria)
            .WithMany(c => c.ProductosCategorias)
            .HasForeignKey(pc => pc.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
