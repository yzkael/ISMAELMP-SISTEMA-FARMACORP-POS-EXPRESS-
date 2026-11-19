using FarmacorpPOS.Domain.Entities.Express.Productos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacorpPOS.Infrastructure.Data.Configurations.Express.Productos;

public class TipoProductoConfiguration : IEntityTypeConfiguration<TipoProducto>
{
    public void Configure(EntityTypeBuilder<TipoProducto> builder)
    {
        builder.ToTable("TiposProducto");
        builder.HasKey(t => t.IdTipoProducto);

        builder.Property(t => t.Descripcion).IsRequired().HasMaxLength(100);
    }
}
