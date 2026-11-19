namespace FarmacorpPOS.ConsoleApp.Utilidades;

// Se lanza cuando el usuario escribe "x" mientras se le piden datos. Corta la
// operación en curso y regresa al menú principal. No deja nada a medias porque
// los servicios recién guardan al final, con todos los datos ya leídos.
public class VolverAlMenuException : Exception
{
}
