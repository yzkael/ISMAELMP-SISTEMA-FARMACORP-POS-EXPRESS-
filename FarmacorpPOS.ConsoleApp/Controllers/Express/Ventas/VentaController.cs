using FarmacorpPOS.ConsoleApp.Utilidades;
using FarmacorpPOS.Services.DTOs.Express.Ventas;
using FarmacorpPOS.Services.Erp.Productos;
using FarmacorpPOS.Services.Express.Ventas;

namespace FarmacorpPOS.ConsoleApp.Controllers.Express.Ventas;

public class VentaController
{
    private readonly IVentaService _ventaService;
    private readonly IProductoErpService _productoService;

    public VentaController(IVentaService ventaService, IProductoErpService productoService)
    {
        _ventaService = ventaService;
        _productoService = productoService;
    }

    public async Task RegistrarAsync()
    {
        Pantalla.Titulo("Registrar venta");
        Pantalla.ComoVolver();

        var productos = await _productoService.ListarAsync();
        Tablas.Productos(productos);
        if (productos.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        var solicitud = new RegistrarVentaSolicitudDTO(
            IdProducto: Entrada.LeerEntero("Id del producto"),
            Cantidad: Entrada.LeerEntero("Cantidad", minimo: 1),
            Cliente: Entrada.LeerTextoOpcional("Cliente (Enter para omitir)"));

        var venta = await _ventaService.RegistrarAsync(solicitud);

        Pantalla.Exito($"Venta #{venta.IdVenta} registrada (estrategia {venta.Estrategia}).");
        Pantalla.Dato("Producto:", venta.Producto);
        Pantalla.Dato("UniqueCodigo:", venta.UniqueProducto.ToString());
        Pantalla.Dato("Cantidad:", venta.Cantidad.ToString());
        Pantalla.Dato("Precio unit.:", Pantalla.Moneda(venta.Precio));
        Pantalla.Dato("Subtotal:", Pantalla.Moneda(venta.Subtotal));
        Pantalla.Dato("Descuento:", venta.Descuento > 0 ? "-" + Pantalla.Moneda(venta.Descuento) : "Sin descuento");
        Pantalla.Dato("Total:", Pantalla.Moneda(venta.Total));
        Pantalla.Dato("Stock restante:", venta.StockRestante.ToString());
    }
}
