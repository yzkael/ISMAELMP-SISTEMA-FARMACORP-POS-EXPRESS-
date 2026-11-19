namespace FarmacorpPOS.Domain.Entities.Express.Categorias;

public class Categoria
{
    public int IdCategoria { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; }

    // Las categorías se pueden anidar (ej. Medicamentos > Analgésicos).
    // Si no tiene padre, es una categoría principal.
    public int? IdCategoriaPadre { get; set; }

    public Categoria? CategoriaPadre { get; set; }
    public ICollection<Categoria> Subcategorias { get; set; } = [];

    public ICollection<ProductoCategoria> ProductosCategorias { get; set; } = [];
}
