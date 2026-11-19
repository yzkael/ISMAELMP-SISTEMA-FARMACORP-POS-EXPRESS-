namespace FarmacorpPOS.Services.DTOs.Express.Ventas;

public record VentaRegistradaDTO(
    int IdVenta,
    string Producto,
    Guid UniqueProducto,
    int Cantidad,
    decimal Precio,
    decimal Subtotal,
    decimal Descuento,
    decimal Total,
    int StockRestante,
    string Estrategia);
