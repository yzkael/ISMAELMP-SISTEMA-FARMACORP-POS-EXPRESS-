namespace FarmacorpPOS.Services.DTOs.Erp.Productos;

// Stock y UniqueCodigo son opcionales porque un producto puede existir en
// Express sin haberse registrado nunca en el ERP.
public record ProductoResumenDTO(
    int IdProducto,
    string Nombre,
    string TipoProducto,
    decimal Precio,
    bool Activo,
    int? Stock,
    Guid? UniqueCodigo,
    int CantidadCategorias,
    int CantidadCodigosBarras);
