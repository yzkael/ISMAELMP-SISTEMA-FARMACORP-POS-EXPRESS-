using FarmacorpPOS.ConsoleApp.Utilidades;
using FarmacorpPOS.Services.DTOs.Erp.Productos;
using FarmacorpPOS.Services.Erp.Productos;

namespace FarmacorpPOS.ConsoleApp.Controllers.Erp.Productos;

public class ProductoErpController
{
    private readonly IProductoErpService _productoService;

    public ProductoErpController(IProductoErpService productoService)
    {
        _productoService = productoService;
    }

    public async Task RegistrarAsync()
    {
        Pantalla.Titulo("Registrar nuevo producto ERP");
        Pantalla.ComoVolver();
        Tablas.TiposProducto(await _productoService.ListarTiposAsync());
        Console.WriteLine();

        // No se piden Precio ni UniqueCodigo: los calcula el sistema.
        var solicitud = new RegistrarProductoErpSolicitudDTO(
            Nombre: Entrada.LeerTexto("Nombre"),
            Costo: Entrada.LeerDecimal("Costo (Bs)"),
            Stock: Entrada.LeerEntero("Stock inicial", minimo: 0),
            FechaVencimiento: Entrada.LeerFecha("Fecha de vencimiento (dd/mm/aaaa)"),
            IdTipoProducto: Entrada.LeerEntero("Id del tipo de producto"),
            Observaciones: Entrada.LeerTextoOpcional("Observaciones (Enter para omitir)"));

        var producto = await _productoService.RegistrarAsync(solicitud);

        Pantalla.Exito($"Producto #{producto.IdProducto} registrado.");
        Pantalla.Dato("Nombre:", producto.Nombre);
        Pantalla.Dato("Costo:", Pantalla.Moneda(producto.Costo));
        Pantalla.Dato("Precio:", $"{Pantalla.Moneda(producto.Precio)}  (Regla 1, estrategia {producto.Estrategia})");
        Pantalla.Dato("UniqueCodigo:", producto.UniqueCodigo.ToString());
        Pantalla.Dato("Stock:", producto.Stock.ToString());
    }

    public async Task ListarAsync()
    {
        Pantalla.Titulo("Productos");
        Tablas.Productos(await _productoService.ListarAsync());
    }
}
