using System.Globalization;
using DonJuanMarket_Ruben_Ibañez.Modelos;
using Spectre.Console;

namespace DonJuanMarket_Ruben_Ibañez.Utilidades
{
    public static class Validaciones
    {
        // Palabra que cancela la operación
        public const string TokenVolver = "x";

        // ---------- Lectura base ----------
        private static string Leer(string mensaje, bool permitirVolver)
        {
            AnsiConsole.Markup($"[bold cyan]{mensaje}[/] ");
            string? linea = Console.ReadLine();

            if (linea == null) throw new EntradaAgotadaException();

            linea = linea.Trim();
            if (permitirVolver && linea.Equals(TokenVolver, StringComparison.OrdinalIgnoreCase))
                throw new OperacionCanceladaException();

            return linea;
        }

        private static void MostrarError(string mensaje) =>
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(mensaje)}[/]");

        // ---------- Texto ----------
        public static string LeerTexto(string mensaje, bool permitirVacio = false, bool permitirVolver = true, int maxLongitud = 60)
        {
            while (true)
            {
                string entrada = Leer(mensaje, permitirVolver);

                if (entrada.Length == 0 && !permitirVacio)
                {
                    MostrarError("Este dato es obligatorio. Intente de nuevo.");
                    continue;
                }
                if (entrada.Length > maxLongitud)
                {
                    MostrarError($"El texto no puede superar los {maxLongitud} caracteres.");
                    continue;
                }
                return entrada;
            }
        }

        // ---------- Números ----------
        // Lee un entero dentro de [minimo, maximo]
        public static int LeerEntero(string mensaje, int minimo, int maximo) =>
            LeerEnteroOpcional(mensaje, minimo, maximo, permitirVacio: false)!.Value;

        public static int? LeerEnteroOpcional(string mensaje, int minimo, int maximo, bool permitirVacio = true)
        {
            while (true)
            {
                string entrada = Leer(mensaje, permitirVolver: true);

                if (entrada.Length == 0)
                {
                    if (permitirVacio) return null;
                    MostrarError("Este dato es obligatorio. Intente de nuevo.");
                    continue;
                }

                if (!int.TryParse(entrada, NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor))
                {
                    MostrarError("Debe ingresar un número entero (sin letras ni decimales).");
                    continue;
                }
                if (valor < 0 && minimo >= 0)
                {
                    MostrarError("El valor no puede ser negativo.");
                    continue;
                }
                if (valor < minimo || valor > maximo)
                {
                    MostrarError($"El valor debe estar entre {minimo:N0} y {maximo:N0}.");
                    continue;
                }
                return valor;
            }
        }

        public static double LeerDecimal(string mensaje, double minimoExclusivo, double maximo) =>
            LeerDecimalOpcional(mensaje, minimoExclusivo, maximo, permitirVacio: false)!.Value;

        public static double? LeerDecimalOpcional(string mensaje, double minimoExclusivo, double maximo, bool permitirVacio = true)
        {
            while (true)
            {
                string entrada = Leer(mensaje, permitirVolver: true);

                if (entrada.Length == 0)
                {
                    if (permitirVacio) return null;
                    MostrarError("Este dato es obligatorio. Intente de nuevo.");
                    continue;
                }

                if (!TryConvertirDecimal(entrada, out double valor))
                {
                    MostrarError("Debe ingresar un número válido (ejemplo: 45.99).");
                    continue;
                }
                if (valor <= minimoExclusivo)
                {
                    MostrarError($"El valor debe ser mayor que {Formato.Numero(minimoExclusivo)}.");
                    continue;
                }
                if (valor > maximo)
                {
                    MostrarError($"El valor no puede superar {Formato.Numero(maximo)}.");
                    continue;
                }
                return valor;
            }
        }

        // globalizar formato "45.99", "45,99", "Q45.99" y "1,234.50"
        private static bool TryConvertirDecimal(string entrada, out double valor)
        {
            string texto = entrada.Trim().TrimStart('Q', 'q').Trim();

            if (texto.Contains(',') && texto.Contains('.'))
                texto = texto.Replace(",", "");
            else
                texto = texto.Replace(',', '.');

            return double.TryParse(texto, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out valor)
                   && !double.IsNaN(valor) && !double.IsInfinity(valor);
        }

        // ---------- Datos del dominio ----------
        // normalizar etiquetas
        public static string? LeerCodigoProducto(string mensaje, bool permitirVacio = false)
        {
            while (true)
            {
                string entrada = Leer(mensaje, permitirVolver: true);

                if (entrada.Length == 0 && permitirVacio) return null;

                if (Producto.TryNormalizarCodigo(entrada, out string codigo))
                    return codigo;

                MostrarError("Código inválido. Use letras/números sin espacios, por ejemplo PROD001 (o solo los dígitos: 1).");
            }
        }

        // Pide el tipo de cliente escribiendo 1, 2, 3 o el nombre (VIP, ADULTO_MAYOR, REGULAR)
        public static TipoCliente LeerTipoCliente(string mensaje)
        {
            while (true)
            {
                string entrada = Leer(mensaje, permitirVolver: true);
                if (TipoClienteExtensiones.TryConvertir(entrada, out TipoCliente tipo))
                    return tipo;

                MostrarError("Tipo inválido. Elija 1 (VIP), 2 (Adulto mayor) o 3 (Regular).");
            }
        }

        // Pregunta sí/no. Acepta s, si, sí, y, yes / n, no
        public static bool Confirmar(string mensaje)
        {
            while (true)
            {
                string entrada = Leer($"{mensaje} (s/n):", permitirVolver: true).ToLowerInvariant();
                switch (entrada)
                {
                    case "s": case "si": case "sí": case "y": case "yes": return true;
                    case "n": case "no": return false;
                    default: MostrarError("Responda con 's' (sí) o 'n' (no)."); break;
                }
            }
        }

        // ---------- Pausa ----------
        public static void ReturnMenu()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Presione una tecla para continuar...[/]");
            try
            {
                Console.ReadKey(true);
            }
            catch (InvalidOperationException)
            {
                if (Console.ReadLine() == null) throw new EntradaAgotadaException();
            }
        }
    }
}
