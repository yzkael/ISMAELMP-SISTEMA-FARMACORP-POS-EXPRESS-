using FarmacorpPOS.ConsoleApp.Utilidades;
using FarmacorpPOS.Services.DTOs.Erp.CodigosBarras;
using FarmacorpPOS.Services.Erp.CodigosBarras;
using FarmacorpPOS.Services.Erp.Productos;

namespace FarmacorpPOS.ConsoleApp.Controllers.Erp.CodigosBarras;

public class CodigoBarraController
{
    private readonly ICodigoBarraService _codigoBarraService;
    private readonly IProductoErpService _productoService;

    public CodigoBarraController(ICodigoBarraService codigoBarraService, IProductoErpService productoService)
    {
        _codigoBarraService = codigoBarraService;
        _productoService = productoService;
    }

    public async Task AsignarAsync()
    {
        Pantalla.Titulo("Asignar código de barras");
        Pantalla.ComoVolver();

        var productos = await _productoService.ListarAsync();
        Tablas.Productos(productos);
        if (productos.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        var solicitud = new AsignarCodigoBarraSolicitudDTO(Entrada.LeerEntero("Id del producto"));

        var codigo = await _codigoBarraService.AsignarAsync(solicitud);

        Pantalla.Exito($"Código de barras #{codigo.IdCodigoBarra} asignado a '{codigo.Producto}'.");
        Pantalla.Dato("UniqueCodigo:", codigo.UniqueCodigo.ToString());
    }
}
