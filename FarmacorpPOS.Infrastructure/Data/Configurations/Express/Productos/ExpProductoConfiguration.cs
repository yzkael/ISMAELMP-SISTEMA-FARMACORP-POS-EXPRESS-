using FarmacorpPOS.Domain.Entities.Express.Productos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacorpPOS.Infrastructure.Data.Configurations.Express.Productos;

public class ExpProductoConfiguration : IEntityTypeConfiguration<ExpProducto>
{
    public void Configure(EntityTypeBuilder<ExpProducto> builder)
    {
        builder.ToTable("ExpProductos");
        builder.HasKey(p => p.IdProducto);

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Precio).HasPrecision(18, 2);
        builder.Property(p => p.FechaVencimiento).HasColumnType("date");
        builder.Property(p => p.Observaciones).HasMaxLength(500);

        // Restrict: no queremos que borrar un tipo se lleve todos sus productos.
        builder.HasOne(p => p.TipoProducto)
            .WithMany(t => t.Productos)
            .HasForeignKey(p => p.IdTipoProducto)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
