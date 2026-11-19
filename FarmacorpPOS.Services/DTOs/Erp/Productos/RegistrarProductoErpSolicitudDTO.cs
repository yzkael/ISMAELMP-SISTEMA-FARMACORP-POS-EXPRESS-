namespace FarmacorpPOS.Services.DTOs.Erp.Productos;

// No incluye Precio ni UniqueCodigo a propósito: los calcula el sistema
// (Regla 1 y generación automática), nunca los decide quien registra.
public record RegistrarProductoErpSolicitudDTO(
    string Nombre,
    decimal Costo,
    int Stock,
    DateTime FechaVencimiento,
    int IdTipoProducto,
    string? Observaciones = null);
