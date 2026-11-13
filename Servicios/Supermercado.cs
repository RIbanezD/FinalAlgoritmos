using System.Globalization;
using DonJuanMarket_Ruben_Ibañez.Estructuras;
using DonJuanMarket_Ruben_Ibañez.Modelos;
using DonJuanMarket_Ruben_Ibañez.Utilidades;

namespace DonJuanMarket_Ruben_Ibañez.Servicios
{
    //   * Inventario de productos 
    //   * Fila de clientes
    public class Supermercado
    {
        public const string NombreTienda = "Don Juan Market";

        private readonly List<Cliente> atendidos = new();
        private long contadorTurnos;

        public IInventario Inventario { get; private set; } = new ArbolAVL();
        public IColaAtencion ColaClientes { get; private set; } = new ColaPrioridad();
        public MapaTienda Mapa { get; } = new();

        // true si hubo altas, bajas o cambios desde el último guardado/importación
        public bool HayCambiosSinGuardar { get; private set; }

        // ---------- INVENTARIO ----------

        // true si se registró; false si el código ya existía
        public bool RegistrarProducto(Producto producto)
        {
            bool insertado = Inventario.Insertar(producto);
            if (insertado) HayCambiosSinGuardar = true;
            return insertado;
        }

        public Producto? BuscarProducto(string codigo) => Inventario.Buscar(codigo);

        // true si el producto existía y fue actualizado
        public bool ActualizarProducto(string codigo, string nombre, string categoria, double precio, int stock)
        {
            bool actualizado = Inventario.Actualizar(codigo, nombre, categoria, precio, stock);
            if (actualizado) HayCambiosSinGuardar = true;
            return actualizado;
        }

        // true si el producto existía y fue eliminado
        public bool EliminarProducto(string codigo)
        {
            bool eliminado = Inventario.Eliminar(codigo);
            if (eliminado) HayCambiosSinGuardar = true;
            return eliminado;
        }

        public List<Producto> ObtenerInventarioOrdenado() => Inventario.ObtenerOrdenado();

        // buscar por nombre
        public List<Producto> BuscarPorNombre(string texto)
        {
            string buscado = Formato.Normalizar(texto);
            return Inventario.ObtenerOrdenado()
                .Where(p => Formato.Normalizar(p.Nombre).Contains(buscado, StringComparison.Ordinal))
                .ToList();
        }

        // buscar por categoria
        public List<Producto> BuscarPorCategoria(string texto)
        {
            string buscada = Formato.Normalizar(texto);
            return Inventario.ObtenerOrdenado()
                .Where(p => Formato.Normalizar(p.Categoria).Contains(buscada, StringComparison.Ordinal))
                .ToList();
        }

        // Siguiente código libre
        public string GenerarSiguienteCodigo()
        {
            int mayor = 0;
            foreach (Producto p in Inventario.ObtenerOrdenado())
            {
                if (p.Codigo.StartsWith(Producto.PrefijoCodigo, StringComparison.Ordinal) &&
                    int.TryParse(p.Codigo.AsSpan(Producto.PrefijoCodigo.Length), NumberStyles.None, CultureInfo.InvariantCulture, out int numero))
                {
                    mayor = Math.Max(mayor, numero);
                }
            }
            return Producto.PrefijoCodigo + (mayor + 1).ToString("D3", CultureInfo.InvariantCulture);
        }

        // ---------- ATENCIÓN DE CLIENTES ----------

        // Registra al cliente en la cola de prioridad
        public int AgregarCliente(Cliente cliente)
        {
            cliente.AsignarTurno(++contadorTurnos);
            ColaClientes.Agregar(cliente);
            HayCambiosSinGuardar = true;
            return ColaClientes.PosicionEstimada(cliente);
        }

        // Consulta al siguiente cliente
        public Cliente? VerSiguienteCliente() => ColaClientes.VerSiguiente();

        // Retira de la cola al cliente de mayor prioridad y lo registra como atendido
        public Cliente? AtenderSiguiente()
        {
            Cliente? cliente = ColaClientes.Extraer();
            if (cliente == null) return null;

            cliente.MarcarAtendido();
            atendidos.Add(cliente);
            HayCambiosSinGuardar = true;
            return cliente;
        }

        // Clientes en espera
        public List<Cliente> ObtenerClientesEnEspera() => ColaClientes.ObtenerEnOrdenDeAtencion();

        public IReadOnlyList<Cliente> ObtenerAtendidos() => atendidos;

        // ---------- IMPORTAR / DEMOSTRACIÓN ----------
        // Incorpora los datos leídos de los archivos CSV. Los códigos repetidos se omiten
        public (int ProductosNuevos, int ProductosDuplicados, int ClientesEnCola, int ClientesAtendidos) Importar(ResultadoImportacion datos)
        {
            int nuevos = 0, duplicados = 0, enCola = 0, yaAtendidos = 0;

            foreach (Producto p in datos.Productos)
            {
                if (Inventario.Insertar(p)) nuevos++; else duplicados++;
            }

            foreach (Cliente c in datos.Clientes.OrderBy(x => x.HoraLlegada))
            {
                c.AsignarTurno(++contadorTurnos);
                if (c.Atendido) { atendidos.Add(c); yaAtendidos++; }
                else { ColaClientes.Agregar(c); enCola++; }
            }

            HayCambiosSinGuardar = false;
            return (nuevos, duplicados, enCola, yaAtendidos);
        }

        public (int Productos, int Clientes) CargarDemostracion()
        {
            var productos = new[]
            {
                new Producto("PROD004", "Detergente Líquido 2L", "Limpieza",    32.75, 25),
                new Producto("PROD002", "Leche Entera 1L",       "Bebidas",     12.50, 100),
                new Producto("PROD006", "Jugo de Naranja 1L",    "Bebidas",      9.95, 60),
                new Producto("PROD001", "Aceite de Oliva 500ml", "Alimentos",   45.99, 50),
                new Producto("PROD003", "Pan Integral",          "Panadería",   15.00, 40),
                new Producto("PROD005", "Audífonos Bluetooth",   "Electrónica", 149.90, 12),
                new Producto("PROD007", "Arroz Blanco 5 lb",     "Alimentos",   28.00, 80),
                new Producto("PROD008", "Escoba de Cerdas",      "Hogar",       18.50, 5),
                new Producto("PROD009", "Cargador USB-C",        "Electrónica", 59.00, 0),
                new Producto("PROD010", "Servilletas x200",      "Hogar",        7.25, 150)
            };

            int productosNuevos = 0;
            foreach (Producto p in productos)
                if (Inventario.Insertar(p)) productosNuevos++;

            DateTime baseHora = DateTime.Now.AddMinutes(-3);
            var clientes = new (string Nombre, TipoCliente Tipo, int Articulos)[]
            {
                ("Juan Pérez",    TipoCliente.Vip,         5),
                ("María López",   TipoCliente.AdultoMayor, 3),
                ("Carlos Gómez",  TipoCliente.Regular,     8),
                ("Ana Morales",   TipoCliente.Regular,     2),
                ("Luis Herrera",  TipoCliente.Vip,        12),
                ("Rosa Castillo", TipoCliente.AdultoMayor, 6)
            };

            for (int i = 0; i < clientes.Length; i++)
                AgregarCliente(new Cliente(clientes[i].Nombre, clientes[i].Tipo, clientes[i].Articulos, baseHora.AddSeconds(30 * i)));

            HayCambiosSinGuardar = true;
            return (productosNuevos, clientes.Length);
        }

        public void MarcarComoGuardado() => HayCambiosSinGuardar = false;

        // ---------- ESTADÍSTICAS ----------
        // Calcula las cifras del reporte
        public EstadisticasSistema ObtenerEstadisticas()
        {
            List<Producto> productos = Inventario.ObtenerOrdenado();
            List<Cliente> enEspera = ColaClientes.ObtenerEnOrdenDeAtencion();

            var categorias = productos
                .GroupBy(p => Formato.Normalizar(p.Categoria))
                .Select(g => new ResumenCategoria(g.First().Categoria, g.Count(), g.Sum(p => p.Stock), g.Sum(p => p.Precio * p.Stock)))
                .OrderByDescending(c => c.Valor)
                .ToList();

            var tiemposEspera = atendidos.Where(c => c.TiempoEspera.HasValue).Select(c => c.TiempoEspera!.Value).ToList();
            var avl = Inventario as ArbolAVL;

            return new EstadisticasSistema
            {
                EstructuraInventario = Inventario.Nombre,
                EstructuraCola = ColaClientes.Nombre,

                TotalProductos = productos.Count,
                UnidadesEnStock = productos.Sum(p => p.Stock),
                ValorInventario = productos.Sum(p => p.Precio * p.Stock),
                ProductosAgotados = productos.Count(p => p.Stock == 0),
                ProductosStockBajo = productos.Count(p => p.Stock > 0 && p.Stock <= EstiloVisual.UmbralStockBajo),
                MasCaro = productos.OrderByDescending(p => p.Precio).FirstOrDefault(),
                MayorStock = productos.OrderByDescending(p => p.Stock).FirstOrDefault(),
                Categorias = categorias,

                AlturaArbol = avl?.Altura,
                RaizArbol = avl?.Raiz?.Producto.Codigo,
                RotacionesSimples = avl?.RotacionesSimples,
                RotacionesDobles = avl?.RotacionesDobles,

                ClientesEnEspera = enEspera.Count,
                EsperaVip = enEspera.Count(c => c.Tipo == TipoCliente.Vip),
                EsperaAdultoMayor = enEspera.Count(c => c.Tipo == TipoCliente.AdultoMayor),
                EsperaRegular = enEspera.Count(c => c.Tipo == TipoCliente.Regular),
                ArticulosPorProcesar = enEspera.Sum(c => c.NumeroProductos),
                SiguienteCliente = enEspera.FirstOrDefault(),

                ClientesAtendidos = atendidos.Count,
                AtendidosVip = atendidos.Count(c => c.Tipo == TipoCliente.Vip),
                AtendidosAdultoMayor = atendidos.Count(c => c.Tipo == TipoCliente.AdultoMayor),
                AtendidosRegular = atendidos.Count(c => c.Tipo == TipoCliente.Regular),
                ArticulosProcesados = atendidos.Sum(c => c.NumeroProductos),
                EsperaPromedio = tiemposEspera.Count > 0 ? TimeSpan.FromTicks((long)tiemposEspera.Average(t => t.Ticks)) : null
            };
        }
    }
}