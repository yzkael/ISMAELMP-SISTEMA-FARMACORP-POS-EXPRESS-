using FarmacorpPOS.Domain.Entities.Express.Productos;

namespace FarmacorpPOS.Domain.Entities.Express.Ventas;

public class VentaExpress
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string? Cliente { get; set; }

    // Guardamos el nombre y el precio tal como estaban al momento de vender.
    // Si mañana el producto cambia de nombre o de precio, la venta tiene que
    // seguir mostrando lo que realmente se cobró.
    public string Producto { get; set; } = string.Empty;
    public decimal Precio { get; set; }

    // Opcional porque las ventas hechas antes del ERP no tienen este código.
    public Guid? UniqueProducto { get; set; }

    public int Cantidad { get; set; }

    // Se guarda el monto y no el porcentaje: el porcentaje depende de la
    // estrategia activa (Base o GanaMax) y lo que importa después es cuánto
    // se descontó de verdad en esta venta.
    public decimal? Descuento { get; set; }

    public decimal Total { get; set; }
    public int IdProducto { get; set; }

    public ExpProducto ExpProducto { get; set; } = null!;
}
