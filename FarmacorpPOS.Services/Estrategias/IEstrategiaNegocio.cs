namespace FarmacorpPOS.Services.Estrategias;

// Agrupa solo las reglas que cambian entre Base y GanaMax. Los servicios
// trabajan contra esta interfaz, así pasar de una estrategia a otra es cambiar
// una línea del appsettings.json y no tocar ningún servicio.
public interface IEstrategiaNegocio
{
    string Nombre { get; }

    // Regla 1
    decimal CalcularPrecio(decimal costo);

    // Regla 3: la condición (una única categoría) es la misma en ambas
    // estrategias; lo único que cambia es el porcentaje.
    decimal PorcentajeDescuentoCategoriaUnica { get; }

    // Regla 4
    void ValidarStock(int stockActual, int cantidadSolicitada);
}
