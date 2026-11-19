using FarmacorpPOS.ConsoleApp.Utilidades;
using FarmacorpPOS.Services.DTOs.Express.Categorias;
using FarmacorpPOS.Services.Erp.Productos;
using FarmacorpPOS.Services.Express.Categorias;

namespace FarmacorpPOS.ConsoleApp.Controllers.Express.Categorias;

public class CategoriaController
{
    private readonly ICategoriaService _categoriaService;
    private readonly IProductoErpService _productoService;

    public CategoriaController(ICategoriaService categoriaService, IProductoErpService productoService)
    {
        _categoriaService = categoriaService;
        _productoService = productoService;
    }

    public async Task AsignarAsync()
    {
        Pantalla.Titulo("Asignar categoría a producto");
        Pantalla.ComoVolver();

        var productos = await _productoService.ListarAsync();
        Tablas.Productos(productos);
        if (productos.Count == 0)
        {
            return;
        }

        Console.WriteLine();
        var idProducto = Entrada.LeerEntero("Id del producto");

        Console.WriteLine();
        Tablas.Categorias(await _categoriaService.ListarAsync());
        Console.WriteLine();
        var idCategoria = Entrada.LeerEntero("Id de la categoría");

        var asignacion = await _categoriaService.AsignarAsync(new AsignarCategoriaSolicitudDTO(idProducto, idCategoria));

        Pantalla.Exito($"Categoría '{asignacion.Categoria}' asignada a '{asignacion.Producto}'.");
        Pantalla.Dato("Categorías:", asignacion.CantidadCategorias.ToString());

        // Se avisa porque asignar una categoría cambia el precio final de las
        // próximas ventas de ese producto, y no es algo obvio para quien asigna.
        if (asignacion.CantidadCategorias == 1)
        {
            Pantalla.Aviso("Con una sola categoría, sus ventas tendrán el descuento de la Regla 3.");
        }
        else if (asignacion.CantidadCategorias == 2)
        {
            Pantalla.Aviso("Con más de una categoría, sus ventas ya no tienen el descuento de la Regla 3.");
        }
    }
}
