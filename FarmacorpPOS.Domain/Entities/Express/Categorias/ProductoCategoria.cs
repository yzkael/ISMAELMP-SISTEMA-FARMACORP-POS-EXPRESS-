using FarmacorpPOS.Domain.Entities.Express.Productos;

namespace FarmacorpPOS.Domain.Entities.Express.Categorias;

// Existe como entidad propia (y no como una simple lista dentro de Producto)
// porque además de unir producto y categoría queremos saber cuándo se hizo
// la asignación.
public class ProductoCategoria
{
    public int IdDetalle { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int IdProducto { get; set; }
    public int IdCategoria { get; set; }

    public ExpProducto Producto { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
}
