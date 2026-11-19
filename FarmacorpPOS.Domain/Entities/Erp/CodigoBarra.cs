using FarmacorpPOS.Domain.Entities.Express.Productos;

namespace FarmacorpPOS.Domain.Entities.Erp;

public class CodigoBarra
{
    public int IdCodigoBarra { get; set; }

    // Lo genera el sistema para evitar códigos repetidos entre productos.
    public Guid UniqueCodigo { get; set; }

    // Un código se desactiva en vez de borrarse, así no se pierde el historial
    // cuando un producto cambia de empaque o de proveedor.
    public bool Activo { get; set; }

    public int IdProducto { get; set; }

    public ExpProducto Producto { get; set; } = null!;
}
