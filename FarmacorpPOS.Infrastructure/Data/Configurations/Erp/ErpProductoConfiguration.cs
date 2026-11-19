using FarmacorpPOS.Domain.Entities.Erp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacorpPOS.Infrastructure.Data.Configurations.Erp;

public class ErpProductoConfiguration : IEntityTypeConfiguration<ErpProducto>
{
    public void Configure(EntityTypeBuilder<ErpProducto> builder)
    {
        // El check es una última barrera: la Regla 4 ya valida el stock en el
        // servicio, pero si algo se escapa, la base no acepta stock negativo.
        builder.ToTable("ErpProductos", t =>
            t.HasCheckConstraint("CK_ErpProductos_Stock", "[Stock] >= 0"));

        builder.HasKey(e => e.IdProducto);

        // El Id viene de ExpProducto, no lo inventa la base: sin esto EF lo
        // trataría como identity y la ficha ERP quedaría con otro Id.
        builder.Property(e => e.IdProducto).ValueGeneratedNever();

        builder.Property(e => e.Costo).HasPrecision(18, 2);
        builder.Property(e => e.UniqueCodigo).IsRequired();
        builder.HasIndex(e => e.UniqueCodigo).IsUnique();

        builder.HasOne(e => e.Producto)
            .WithOne(p => p.ErpProducto)
            .HasForeignKey<ErpProducto>(e => e.IdProducto)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
