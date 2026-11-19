using FarmacorpPOS.Services.Excepciones;

namespace FarmacorpPOS.Services.Estrategias;

// Estrategia temporal de 3 meses. Las reglas que no aparecen aquí (2 y 5)
// son iguales a las de Base y viven directamente en los servicios.
public class EstrategiaGanaMax : IEstrategiaNegocio
{
    private const int StockMinimoDespuesDeVenta = 10;

    public string Nombre => "GanaMax";

    public decimal CalcularPrecio(decimal costo) =>
        Math.Round(costo * 1.80m, 2, MidpointRounding.AwayFromZero);

    public decimal PorcentajeDescuentoCategoriaUnica => 0.10m;

    public void ValidarStock(int stockActual, int cantidadSolicitada)
    {
        if (cantidadSolicitada > stockActual)
        {
            throw new ReglaNegocioException(
                $"Stock insuficiente: hay {stockActual} unidades y se pidieron {cantidadSolicitada}.");
        }

        var stockRestante = stockActual - cantidadSolicitada;
        if (stockRestante <= StockMinimoDespuesDeVenta)
        {
            throw new ReglaNegocioException(
                $"Con la estrategia GanaMax la venta debe dejar más de {StockMinimoDespuesDeVenta} unidades en stock, y quedarían {stockRestante}.");
        }
    }
}
