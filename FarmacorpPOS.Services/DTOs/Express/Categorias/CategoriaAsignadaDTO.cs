namespace FarmacorpPOS.Services.DTOs.Express.Categorias;

// CantidadCategorias se devuelve para que quien asigna vea enseguida si el
// producto quedó con una sola categoría, que es lo que activa la Regla 3.
public record CategoriaAsignadaDTO(int IdDetalle, string Producto, string Categoria, int CantidadCategorias);
