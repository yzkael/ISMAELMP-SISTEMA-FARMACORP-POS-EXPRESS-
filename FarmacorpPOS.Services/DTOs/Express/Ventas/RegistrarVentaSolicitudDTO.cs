namespace FarmacorpPOS.Services.DTOs.Express.Ventas;

// Un solo producto por venta, como pide la Venta Express.
public record RegistrarVentaSolicitudDTO(int IdProducto, int Cantidad, string? Cliente = null);
