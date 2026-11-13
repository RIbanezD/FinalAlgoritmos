using System.Globalization;
using System.Text.RegularExpressions;

namespace DonJuanMarket_Ruben_Ibañez.Modelos
{
    public class Producto
    {
        public const string PrefijoCodigo = "PROD";
        private static readonly Regex PatronCodigo = new(@"^[A-Z0-9_-]{3,15}$", RegexOptions.Compiled);

        private string nombre = string.Empty;
        private string categoria = string.Empty;
        private double precio;
        private int stock;

        // Código producto
        public string Codigo { get; }

        public string Nombre
        {
            get => nombre;
            set => nombre = ValidarTexto(value, "El nombre del producto");
        }

        public string Categoria
        {
            get => categoria;
            set => categoria = ValidarTexto(value, "La categoría");
        }

        // Precio unitario
        public double Precio
        {
            get => precio;
            set => precio = ValidarPrecio(value);
        }

        // Unidades disponibles
        public int Stock
        {
            get => stock;
            set => stock = ValidarStock(value);
        }

        public Producto(string codigo, string nombre, string categoria, double precio, int stock)
        {
            if (!TryNormalizarCodigo(codigo, out string normalizado))
                throw new ArgumentException($"El código '{codigo}' no es válido.", nameof(codigo));

            Codigo = normalizado;
            Nombre = nombre;
            Categoria = categoria;
            Precio = precio;
            Stock = stock;
        }

        /// Modifica nombre, categoría, precio y stock
        public void Actualizar(string nuevoNombre, string nuevaCategoria, double nuevoPrecio, int nuevoStock)
        {
            string n = ValidarTexto(nuevoNombre, "El nombre del producto");
            string c = ValidarTexto(nuevaCategoria, "La categoría");
            double p = ValidarPrecio(nuevoPrecio);
            int s = ValidarStock(nuevoStock);

            nombre = n;
            categoria = c;
            precio = p;
            stock = s;
        }

        // Normaliza el código escrito por el usuario
        public static bool TryNormalizarCodigo(string? entrada, out string codigo)
        {
            codigo = string.Empty;
            if (string.IsNullOrWhiteSpace(entrada)) return false;

            string limpio = entrada.Trim().ToUpperInvariant();

            if (limpio.All(char.IsAsciiDigit))
            {
                if (limpio.Length > 6 || !int.TryParse(limpio, NumberStyles.None, CultureInfo.InvariantCulture, out int numero) || numero <= 0)
                    return false;
                limpio = PrefijoCodigo + numero.ToString("D3", CultureInfo.InvariantCulture);
            }

            if (!PatronCodigo.IsMatch(limpio)) return false;
            codigo = limpio;
            return true;
        }

        private static string ValidarTexto(string? valor, string campo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"{campo} no puede estar vacío.");
            string limpio = valor.Trim();
            if (limpio.Length > 60)
                throw new ArgumentException($"{campo} no puede superar los 60 caracteres.");
            return limpio;
        }

        private static double ValidarPrecio(double valor)
        {
            if (double.IsNaN(valor) || double.IsInfinity(valor) || valor <= 0)
                throw new ArgumentOutOfRangeException(nameof(Precio), "El precio debe ser mayor que 0.");
            return valor;
        }

        private static int ValidarStock(int valor)
        {
            if (valor < 0)
                throw new ArgumentOutOfRangeException(nameof(Stock), "La cantidad en stock debe ser mayor o igual a 0.");
            return valor;
        }

        public override string ToString() =>
            $"{Codigo} | {Nombre} | {Categoria} | Q{Precio.ToString("0.00", CultureInfo.InvariantCulture)} | {Stock}";
    }
}
