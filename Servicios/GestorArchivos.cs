using System.Globalization;
using System.Text;
using DonJuanMarket_Ruben_Ibañez.Modelos;

namespace DonJuanMarket_Ruben_Ibañez.Servicios
{
    public class ResultadoImportacion
    {
        public List<Producto> Productos { get; } = new();
        public List<Cliente> Clientes { get; } = new();
        public List<string> Errores { get; } = new();
    }

    // Guarda y recupera los datos en archivos CSV
    //   inventario.csv → Codigo,Nombre,Categoria,Precio,Stock
    //   clientes.csv   → Nombre,Tipo,Productos,HoraLlegada,HoraAtencion (HoraAtencion vacía = sigue en espera)
    public static class GestorArchivos
    {
        private const string EncabezadoInventario = "Codigo,Nombre,Categoria,Precio,Stock";
        private const string EncabezadoClientes = "Nombre,Tipo,Productos,HoraLlegada,HoraAtencion";

        public static string RutaInventario => Path.Combine(AppContext.BaseDirectory, "inventario.csv");
        public static string RutaClientes => Path.Combine(AppContext.BaseDirectory, "clientes.csv");

        public static bool ExistenDatos() => File.Exists(RutaInventario) || File.Exists(RutaClientes);

        // ---------- GUARDAR ----------
        public static void Guardar(IEnumerable<Producto> productos, IEnumerable<Cliente> clientes)
        {
            var utf8ConBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true); // Excel reconoce tildes y ñ

            using (var escritor = new StreamWriter(RutaInventario, false, utf8ConBom))
            {
                escritor.WriteLine(EncabezadoInventario);
                foreach (Producto p in productos)
                {
                    escritor.WriteLine(string.Join(",",
                        Escapar(p.Codigo),
                        Escapar(p.Nombre),
                        Escapar(p.Categoria),
                        p.Precio.ToString("R", CultureInfo.InvariantCulture),
                        p.Stock.ToString(CultureInfo.InvariantCulture)));
                }
            }

            using (var escritor = new StreamWriter(RutaClientes, false, utf8ConBom))
            {
                escritor.WriteLine(EncabezadoClientes);
                foreach (Cliente c in clientes)
                {
                    escritor.WriteLine(string.Join(",",
                        Escapar(c.Nombre),
                        c.Tipo.ATexto(),
                        c.NumeroProductos.ToString(CultureInfo.InvariantCulture),
                        c.HoraLlegada.ToString("o", CultureInfo.InvariantCulture),
                        c.HoraAtencion?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty));
                }
            }
        }

        // ---------- IMPORTAR ----------
        public static ResultadoImportacion Importar()
        {
            var resultado = new ResultadoImportacion();

            if (File.Exists(RutaInventario))
                LeerArchivo(RutaInventario, "inventario.csv", resultado.Errores, (campos, error) =>
                {
                    if (campos.Count != 5) { error("se esperaban 5 columnas."); return; }
                    if (!double.TryParse(campos[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double precio)) { error("precio inválido."); return; }
                    if (!int.TryParse(campos[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int stock)) { error("stock inválido."); return; }

                    try { resultado.Productos.Add(new Producto(campos[0], campos[1], campos[2], precio, stock)); }
                    catch (ArgumentException ex) { error(ex.Message); }
                });

            if (File.Exists(RutaClientes))
                LeerArchivo(RutaClientes, "clientes.csv", resultado.Errores, (campos, error) =>
                {
                    if (campos.Count != 5) { error("se esperaban 5 columnas."); return; }
                    if (!TipoClienteExtensiones.TryConvertir(campos[1], out TipoCliente tipo)) { error("tipo de cliente inválido."); return; }
                    if (!int.TryParse(campos[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int articulos)) { error("número de productos inválido."); return; }
                    if (!DateTime.TryParse(campos[3], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime llegada)) { error("hora de llegada inválida."); return; }

                    DateTime? atencion = null;
                    if (campos[4].Length > 0)
                    {
                        if (!DateTime.TryParse(campos[4], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime hora)) { error("hora de atención inválida."); return; }
                        atencion = hora;
                    }

                    try
                    {
                        var cliente = new Cliente(campos[0], tipo, articulos, llegada);
                        if (atencion.HasValue) cliente.MarcarAtendido(atencion);
                        resultado.Clientes.Add(cliente);
                    }
                    catch (ArgumentException ex) { error(ex.Message); }
                });

            return resultado;
        }

        private static void LeerArchivo(string ruta, string nombre, List<string> errores,
                                        Action<List<string>, Action<string>> procesar)
        {
            string[] lineas = File.ReadAllLines(ruta, Encoding.UTF8);
            for (int i = 1; i < lineas.Length; i++) // la línea 0 es el encabezado
            {
                if (string.IsNullOrWhiteSpace(lineas[i])) continue;

                int numeroLinea = i + 1;
                procesar(DividirLinea(lineas[i]), motivo => errores.Add($"{nombre}, línea {numeroLinea}: {motivo}"));
            }
        }

        // ---------- CSV básico ----------
        private static string Escapar(string texto)
        {
            if (texto.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0) return texto;
            return "\"" + texto.Replace("\"", "\"\"") + "\"";
        }

        private static List<string> DividirLinea(string linea)
        {
            var campos = new List<string>();
            var actual = new StringBuilder();
            bool entreComillas = false;

            for (int i = 0; i < linea.Length; i++)
            {
                char c = linea[i];
                if (entreComillas)
                {
                    if (c == '"' && i + 1 < linea.Length && linea[i + 1] == '"') { actual.Append('"'); i++; }
                    else if (c == '"') entreComillas = false;
                    else actual.Append(c);
                }
                else if (c == '"') entreComillas = true;
                else if (c == ',') { campos.Add(actual.ToString().Trim()); actual.Clear(); }
                else actual.Append(c);
            }
            campos.Add(actual.ToString().Trim());
            return campos;
        }
    }
}
