using System.Globalization;
using System.Text;

namespace DonJuanMarket_Ruben_Ibañez.Utilidades
{
    public static class Formato
    {
        public static string Moneda(double valor) =>
            "Q" + valor.ToString("N2", CultureInfo.InvariantCulture);

        public static string Fecha(DateTime fecha) =>
            fecha.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        public static string Hora(DateTime fecha) =>
            fecha.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        public static string Numero(double valor) =>
            valor.ToString("0.##", CultureInfo.InvariantCulture);

        public static string Duracion(TimeSpan tiempo)
        {
            if (tiempo.TotalHours >= 1) return $"{(int)tiempo.TotalHours} h {tiempo.Minutes} min";
            if (tiempo.TotalMinutes >= 1) return $"{(int)tiempo.TotalMinutes} min {tiempo.Seconds} s";
            return $"{tiempo.Seconds} s";
        }

        // normalizar
        public static string Normalizar(string texto)
        {
            string descompuesto = texto.Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompuesto.Length);
            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        // truncar datos
        public static string Truncar(string texto, int maximo) =>
            texto.Length <= maximo ? texto : texto[..(maximo - 1)] + "…";
    }

    public class OperacionCanceladaException : Exception
    {
        public OperacionCanceladaException() : base("Operación cancelada por el usuario.") { }
    }

    public class EntradaAgotadaException : Exception
    {
        public EntradaAgotadaException() : base("No hay más datos de entrada disponibles.") { }
    }
}
