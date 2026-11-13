using DonJuanMarket_Ruben_Ibañez.Modelos;

namespace DonJuanMarket_Ruben_Ibañez.Estructuras
{
    public class GrafoInventario : IInventario
    {
        private readonly Grafo<Producto> grafo = new(dirigido: true);
        private readonly List<string> bitacora = new();
        private long comparaciones;

        public string Nombre => "Grafo dirigido de precedencia (DAG)";
        public string Etiqueta => "GRAFO";
        public string ResumenComplejidad => "";

        public int Cantidad => grafo.CantidadVertices;
        public long Comparaciones => comparaciones;
        public IReadOnlyList<string> UltimaOperacion => bitacora;

        public Grafo<Producto> Grafo => grafo;

        // ---------- INSERTAR ----------
        public bool Insertar(Producto producto)
        {
            ArgumentNullException.ThrowIfNull(producto);
            bitacora.Clear();

            // Primera pasada: se compara el código nuevo contra CADA vértice existente. Esa misma comparación sirve para detectar duplicados
            var anteriores = new List<Vertice<Producto>>();   // códigos menores  → arista existente → nuevo
            var posteriores = new List<Vertice<Producto>>();  // códigos mayores → arista nuevo → existente

            foreach (var existente in grafo.Vertices)
            {
                comparaciones++;
                int comparacion = string.CompareOrdinal(existente.Dato.Codigo, producto.Codigo);

                if (comparacion == 0) return false; // código duplicado
                if (comparacion < 0) anteriores.Add(existente); else posteriores.Add(existente);
            }

            var nuevo = grafo.AgregarVertice(producto, producto.Codigo);
            foreach (var v in anteriores) grafo.AgregarArista(v, nuevo);
            foreach (var v in posteriores) grafo.AgregarArista(nuevo, v);

            bitacora.Add($"Vértice {producto.Codigo} agregado; se crearon {anteriores.Count + posteriores.Count} arista(s) de precedencia " +
                         $"({nuevo.GradoEntrada} entrantes, {nuevo.GradoSalida} salientes).");
            return true;
        }

        // ---------- BUSCAR ----------
        public Producto? Buscar(string codigo) => BuscarVertice(codigo)?.Dato;

        // BFS hasta hallar el vértice con ese código
        private Vertice<Producto>? BuscarVertice(string codigo) =>
            grafo.BuscarBFS(p =>
            {
                comparaciones++;
                return string.Equals(p.Codigo, codigo, StringComparison.Ordinal);
            });

        // ---------- ACTUALIZAR / ELIMINAR ----------
        public bool Actualizar(string codigo, string nombre, string categoria, double precio, int stock)
        {
            bitacora.Clear();
            var vertice = BuscarVertice(codigo);
            if (vertice == null) return false;

            vertice.Dato.Actualizar(nombre, categoria, precio, stock);
            bitacora.Add("El código no cambió: el grafo no se modifica.");
            return true;
        }

        public bool Eliminar(string codigo)
        {
            bitacora.Clear();
            var vertice = BuscarVertice(codigo);
            if (vertice == null) return false;

            int aristas = vertice.GradoEntrada + vertice.GradoSalida;
            grafo.EliminarVertice(vertice);
            bitacora.Add($"Vértice {codigo} eliminado junto con sus {aristas} arista(s). " +
                         "El orden de los demás no se altera (la precedencia es transitiva).");
            return true;
        }

        // ---------- ORDENADO ----------
        // Inventario ordenado por código mediante orden topológico (Kahn)
        public List<Producto> ObtenerOrdenado()
        {
            var orden = grafo.OrdenTopologico()
                ?? throw new InvalidOperationException("El grafo de inventario contiene un ciclo, lo cual no debería ocurrir.");
            return orden.Select(v => v.Dato).ToList();
        }

        public void ReiniciarContadores() => comparaciones = 0;

        public void Vaciar()
        {
            grafo.Vaciar();
            bitacora.Clear();
        }
    }
}
