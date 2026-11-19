using FarmacorpPOS.Services.DTOs.Erp.Productos;
using FarmacorpPOS.Services.DTOs.Express.Categorias;

namespace FarmacorpPOS.ConsoleApp.Utilidades;

// Varias opciones del menú necesitan mostrar las mismas listas antes de pedir
// un Id, así que las tablas se dibujan en un solo lugar.
public static class Tablas
{
    public static void Productos(IReadOnlyList<ProductoResumenDTO> productos)
    {
        if (productos.Count == 0)
        {
            Pantalla.Aviso("No hay productos registrados. Registra uno con la opción 1.");
            return;
        }

        Console.WriteLine($"  {"Id",-4} {"Nombre",-24} {"Tipo",-17} {"Precio",11} {"Stock",6} {"Categ.",7} {"Cód.",5}");
        foreach (var p in productos)
        {
            var stock = p.Stock?.ToString() ?? "-";
            var nombre = p.Activo ? p.Nombre : p.Nombre + " (inactivo)";
            Console.WriteLine($"  {p.IdProducto,-4} {Recortar(nombre, 24),-24} {Recortar(p.TipoProducto, 17),-17} {Pantalla.Moneda(p.Precio),11} {stock,6} {p.CantidadCategorias,7} {p.CantidadCodigosBarras,5}");
        }
    }

    public static void Categorias(IReadOnlyList<CategoriaDTO> categorias)
    {
        Console.WriteLine($"  {"Id",-4} {"Categoría",-20} {"Pertenece a",-20} {"Estado",-8}");
        foreach (var c in categorias)
        {
            Console.WriteLine($"  {c.IdCategoria,-4} {Recortar(c.Descripcion, 20),-20} {Recortar(c.CategoriaPadre ?? "-", 20),-20} {(c.Activo ? "Activa" : "Inactiva"),-8}");
        }
    }

    public static void TiposProducto(IReadOnlyList<TipoProductoDTO> tipos)
    {
        Console.WriteLine($"  {"Id",-4} {"Tipo de producto",-20}");
        foreach (var t in tipos)
        {
            Console.WriteLine($"  {t.IdTipoProducto,-4} {t.Descripcion,-20}");
        }
    }

    private static string Recortar(string texto, int largo) =>
        texto.Length <= largo ? texto : texto[..(largo - 1)] + "…";
}
