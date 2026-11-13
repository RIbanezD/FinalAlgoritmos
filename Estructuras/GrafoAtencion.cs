using DonJuanMarket_Ruben_Ibañez.Modelos;

namespace DonJuanMarket_Ruben_Ibañez.Estructuras
{
    public class GrafoAtencion : IColaAtencion
    {
        private readonly Grafo<Cliente> grafo = new(dirigido: true);
        private readonly List<string> bitacora = new();
        private long comparaciones;

        public string Nombre => "Grafo dirigido de precedencia (DAG)";
        public string Etiqueta => "GRAFO";
        public string ResumenComplejidad => "Agregar y extraer: O(n) · Ver la fila (Kahn): O(V+E)";

        public int Cantidad => grafo.CantidadVertices;
        public bool EstaVacia => grafo.CantidadVertices == 0;
        public long Comparaciones => comparaciones;
        public IReadOnlyList<string> UltimaOperacion => bitacora;

        // Grafo interno para poder dibujarlo
        public Grafo<Cliente> Grafo => grafo;

        // ---------- AGREGAR ----------
        public void Agregar(Cliente cliente)
        {
            ArgumentNullException.ThrowIfNull(cliente);
            bitacora.Clear();

            var nuevo = grafo.AgregarVertice(cliente, cliente.Nombre);
            int aristasCreadas = 0;

            foreach (var existente in grafo.Vertices)
            {
                if (existente == nuevo) continue;

                comparaciones++;
                if (existente.Dato.TienePrioridadSobre(cliente))
                    grafo.AgregarArista(existente, nuevo);
                else
                    grafo.AgregarArista(nuevo, existente);
                aristasCreadas++;
            }

            bitacora.Add($"Vértice '{cliente.Nombre}' agregado; se crearon {aristasCreadas} arista(s) de precedencia " +
                         $"({nuevo.GradoEntrada} clientes van antes que él).");
        }

        // ---------- EXTRAER / CONSULTAR ----------
        public Cliente? Extraer()
        {
            bitacora.Clear();
            var fuente = BuscarFuente();
            if (fuente == null) return null;

            int aristas = fuente.GradoSalida;
            var cliente = fuente.Dato;
            grafo.EliminarVertice(fuente);

            bitacora.Add($"Se extrajo a '{cliente.Nombre}' (vértice fuente, grado de entrada 0) y se eliminaron sus {aristas} arista(s) salientes.");
            return cliente;
        }

        public Cliente? VerSiguiente() => BuscarFuente()?.Dato;

        private Vertice<Cliente>? BuscarFuente()
        {
            foreach (var vertice in grafo.Vertices)
            {
                comparaciones++;
                if (vertice.GradoEntrada == 0) return vertice;
            }
            return null;
        }

        public int PosicionEstimada(Cliente cliente)
        {
            foreach (var vertice in grafo.Vertices)
            {
                if (ReferenceEquals(vertice.Dato, cliente))
                    return vertice.GradoEntrada + 1;
            }
            return -1;
        }

        public List<Cliente> ObtenerEnOrdenDeAtencion()
        {
            var orden = grafo.OrdenTopologico()
                ?? throw new InvalidOperationException("El grafo de atención contiene un ciclo, lo cual no debería ocurrir.");
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
