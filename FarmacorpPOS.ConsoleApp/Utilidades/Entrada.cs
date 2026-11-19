using System.Globalization;

namespace FarmacorpPOS.ConsoleApp.Utilidades;

// Toda la lectura de datos pasa por aquí para que los Controllers no repitan
// validaciones de formato: cada método insiste hasta recibir un valor válido.
public static class Entrada
{
    public const string ComandoVolver = "x";

    public static string LeerTexto(string etiqueta)
    {
        while (true)
        {
            var valor = LeerLinea(etiqueta);
            if (!string.IsNullOrWhiteSpace(valor))
            {
                return valor.Trim();
            }

            Pantalla.Error("Este dato es obligatorio.");
        }
    }

    public static string? LeerTextoOpcional(string etiqueta)
    {
        var valor = LeerLinea(etiqueta);
        return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
    }

    public static int LeerEntero(string etiqueta, int minimo = int.MinValue)
    {
        while (true)
        {
            if (int.TryParse(LeerLinea(etiqueta), out var valor) && valor >= minimo)
            {
                return valor;
            }

            Pantalla.Error(minimo == int.MinValue
                ? "Ingresa un número entero."
                : $"Ingresa un número entero mayor o igual a {minimo}.");
        }
    }

    // Acepta coma o punto como separador decimal, porque en Bolivia es común
    // escribir "10,50" y no queremos rechazarlo por un detalle de formato.
    public static decimal LeerDecimal(string etiqueta)
    {
        while (true)
        {
            var texto = LeerLinea(etiqueta).Replace(',', '.');
            if (decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var valor))
            {
                return valor;
            }

            Pantalla.Error("Ingresa un monto válido, por ejemplo 10.50");
        }
    }

    public static DateTime LeerFecha(string etiqueta)
    {
        while (true)
        {
            if (DateTime.TryParseExact(LeerLinea(etiqueta), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            {
                return fecha;
            }

            Pantalla.Error("Ingresa la fecha con el formato dd/mm/aaaa, por ejemplo 31/12/2027");
        }
    }

    // Si la entrada se termina (Ctrl+D, o un archivo redirigido que se acabó),
    // se cancela la operación en vez de quedar pidiendo datos para siempre.
    private static string LeerLinea(string etiqueta)
    {
        Console.Write($"{etiqueta}: ");
        var linea = Console.ReadLine() ?? throw new OperationCanceledException("Se terminó la entrada de datos.");

        if (linea.Trim().Equals(ComandoVolver, StringComparison.OrdinalIgnoreCase))
        {
            throw new VolverAlMenuException();
        }

        return linea;
    }
}
