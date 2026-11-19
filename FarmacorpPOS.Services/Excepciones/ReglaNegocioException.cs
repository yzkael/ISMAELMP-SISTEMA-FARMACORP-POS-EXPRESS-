namespace FarmacorpPOS.Services.Excepciones;

// Distingue "el usuario pidió algo que el negocio no permite" de un error real
// del sistema. Los controllers muestran el mensaje de esta excepción tal cual,
// y cualquier otra la tratan como una falla inesperada.
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje)
    {
    }
}
