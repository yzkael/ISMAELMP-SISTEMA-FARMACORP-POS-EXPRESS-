using FarmacorpPOS.Services.Excepciones;

namespace FarmacorpPOS.Services.Estrategias;

public class EstrategiaBase : IEstrategiaNegocio
{
    public string Nombre => "Base";

    public decimal CalcularPrecio(decimal costo) =>
        Math.Round(costo * 1.50m, 2, MidpointRounding.AwayFromZero);

    public decimal PorcentajeDescuentoCategoriaUnica => 0.30m;

    public void ValidarStock(int stockActual, int cantidadSolicitada)
    {
        if (cantidadSolicitada > stockActual)
        {
            throw new ReglaNegocioException(
                $"Stock insuficiente: hay {stockActual} unidades y se pidieron {cantidadSolicitada}.");
        }
    }
}
