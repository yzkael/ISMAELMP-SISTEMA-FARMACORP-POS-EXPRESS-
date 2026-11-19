namespace FarmacorpPOS.Services.DTOs.Erp.Productos;

public record ProductoErpRegistradoDTO(
    int IdProducto,
    string Nombre,
    decimal Costo,
    decimal Precio,
    Guid UniqueCodigo,
    int Stock,
    string Estrategia);
