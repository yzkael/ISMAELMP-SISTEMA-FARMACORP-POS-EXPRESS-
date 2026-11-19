using FarmacorpPOS.Domain.Entities.Express.Ventas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacorpPOS.Infrastructure.Data.Configurations.Express.Ventas;

public class VentaExpressConfiguration : IEntityTypeConfiguration<VentaExpress>
{
    public void Configure(EntityTypeBuilder<VentaExpress> builder)
    {
        builder.ToTable("VentaExpress");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Cliente).HasMaxLength(150);
        builder.Property(v => v.Producto).IsRequired().HasMaxLength(150);
        builder.Property(v => v.Precio).HasPrecision(18, 2);
        builder.Property(v => v.Descuento).HasPrecision(18, 2);
        builder.Property(v => v.Total).HasPrecision(18, 2);

        // Las ventas son historial contable: un producto que ya se vendió
        // no debe poder borrarse y arrastrar sus ventas con él.
        builder.HasOne(v => v.ExpProducto)
            .WithMany(p => p.Ventas)
            .HasForeignKey(v => v.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
