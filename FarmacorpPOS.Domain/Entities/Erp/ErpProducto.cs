using FarmacorpPOS.Domain.Entities.Express.Productos;

namespace FarmacorpPOS.Domain.Entities.Erp;

// El ERP llegó después que el módulo Express, por eso su información está
// separada: Express puede seguir vendiendo sin saber nada de costos ni stock.
// Usa el mismo Id que ExpProducto porque un producto tiene una sola ficha ERP.
public class ErpProducto
{
    public int IdProducto { get; set; }
    public decimal Costo { get; set; }

    // Lo genera el sistema para que nunca haya dos productos con el mismo código.
    public Guid UniqueCodigo { get; set; }

    public DateTime FechaRegistro { get; set; }
    public int Stock { get; set; }

    public ExpProducto Producto { get; set; } = null!;
}
