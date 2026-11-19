using FarmacorpPOS.Domain.Entities.Erp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacorpPOS.Infrastructure.Data.Configurations.Erp;

public class CodigoBarraConfiguration : IEntityTypeConfiguration<CodigoBarra>
{
    public void Configure(EntityTypeBuilder<CodigoBarra> builder)
    {
        builder.ToTable("CodigosBarras");
        builder.HasKey(c => c.IdCodigoBarra);

        builder.Property(c => c.UniqueCodigo).IsRequired();
        builder.HasIndex(c => c.UniqueCodigo).IsUnique();

        builder.HasOne(c => c.Producto)
            .WithMany(p => p.CodigosBarras)
            .HasForeignKey(c => c.IdProducto)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
