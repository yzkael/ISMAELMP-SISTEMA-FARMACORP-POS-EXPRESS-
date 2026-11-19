using System.Globalization;

namespace FarmacorpPOS.ConsoleApp.Utilidades;

public static class Pantalla
{
    public static void Titulo(string texto)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {texto} ---");
    }

    public static void Exito(string texto) => EscribirEnColor(texto, ConsoleColor.Green);

    public static void Error(string texto) => EscribirEnColor(texto, ConsoleColor.Red);

    public static void Aviso(string texto) => EscribirEnColor(texto, ConsoleColor.Yellow);

    // Se muestra al entrar a cada opción que pide datos, para que nadie quede
    // atrapado en una pantalla sin saber cómo salir.
    public static void ComoVolver() =>
        EscribirEnColor($"(Escribe '{Entrada.ComandoVolver}' en cualquier dato para volver al menú)", ConsoleColor.DarkGray);

    public static void Dato(string etiqueta, string valor) => Console.WriteLine($"  {etiqueta,-16} {valor}");

    // Se formatea siempre con punto decimal para que los montos se vean igual
    // en cualquier máquina, sin depender de su configuración regional.
    public static string Moneda(decimal monto) => "Bs " + monto.ToString("0.00", CultureInfo.InvariantCulture);

    private static void EscribirEnColor(string texto, ConsoleColor color)
    {
        var anterior = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(texto);
        Console.ForegroundColor = anterior;
    }
}
