namespace FarmacorpPOS.Services.DTOs.Express.Categorias;

public record CategoriaDTO(int IdCategoria, string Descripcion, bool Activo, string? CategoriaPadre);
