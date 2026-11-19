using FarmacorpPOS.Domain.Entities.Express.Categorias;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmacorpPOS.Infrastructure.Data.Configurations.Express.Categorias;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(c => c.IdCategoria);

        builder.Property(c => c.Descripcion).IsRequired().HasMaxLength(100);

        // SQL Server no acepta borrado en cascada sobre una tabla que se apunta
        // a sí misma, y de todas formas no queremos perder subcategorías por
        // borrar al padre.
        builder.HasOne(c => c.CategoriaPadre)
            .WithMany(c => c.Subcategorias)
            .HasForeignKey(c => c.IdCategoriaPadre)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
