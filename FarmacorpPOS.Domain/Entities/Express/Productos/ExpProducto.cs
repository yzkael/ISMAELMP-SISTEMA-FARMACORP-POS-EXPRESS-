using FarmacorpPOS.Domain.Entities.Erp;
using FarmacorpPOS.Domain.Entities.Express.Categorias;
using FarmacorpPOS.Domain.Entities.Express.Ventas;

namespace FarmacorpPOS.Domain.Entities.Express.Productos;

// Es el producto "original", de cuando el sistema solo hacía Venta Express.
// Todo lo que trajo el ERP (costo, stock, códigos) vive en otras tablas para
// no tener que modificar esta y no romper lo que ya funcionaba.
public class ExpProducto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = null!;
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string? Observaciones { get; set; }
    public int IdTipoProducto { get; set; }

    public TipoProducto TipoProducto { get; set; } = null!;

    // Puede venir vacío: hay productos que se crearon antes del ERP
    // y nunca se registraron en el inventario.
    public ErpProducto? ErpProducto { get; set; }

    public ICollection<ProductoCategoria> ProductosCategorias { get; set; } = [];
    public ICollection<CodigoBarra> CodigosBarras { get; set; } = [];
    public ICollection<VentaExpress> Ventas { get; set; } = [];
}
