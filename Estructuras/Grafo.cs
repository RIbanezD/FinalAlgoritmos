using System.Globalization;

namespace DonJuanMarket_Ruben_Ibañez.Estructuras
{
    public sealed class Arista<T> where T : class
    {
        public Vertice<T> Origen { get; }
        public Vertice<T> Destino { get; }
        public double Peso { get; }

        internal Arista(Vertice<T> origen, Vertice<T> destino, double peso)
        {
            Origen = origen;
            Destino = destino;
            Peso = peso;
        }
    }

    public sealed class Vertice<T> where T : class
    {
        private readonly Dictionary<Vertice<T>, Arista<T>> salientes = new();
        private readonly Dictionary<Vertice<T>, Arista<T>> entrantes = new();

        public int Id { get; }
        public T Dato { get; }

        // Texto corto que identifica al vértice en pantalla (ej.: "PROD001", "Bebidas")
        public string Etiqueta { get; }

        public IEnumerable<Arista<T>> Salientes => salientes.Values;
        public IEnumerable<Arista<T>> Entrantes => entrantes.Values;

        // Cantidad de aristas que salen del vértice (grado de salida)
        public int GradoSalida => salientes.Count;

        // Cantidad de aristas que llegan al vértice (grado de entrada)
        public int GradoEntrada => entrantes.Count;

        internal Vertice(int id, T dato, string etiqueta)
        {
            Id = id;
            Dato = dato;
            Etiqueta = etiqueta;
        }

        internal void AgregarSaliente(Arista<T> arista) => salientes[arista.Destino] = arista;
        internal void AgregarEntrante(Arista<T> arista) => entrantes[arista.Origen] = arista;
        internal void QuitarSaliente(Vertice<T> destino) => salientes.Remove(destino);
        internal void QuitarEntrante(Vertice<T> origen) => entrantes.Remove(origen);
        internal bool TieneSaliente(Vertice<T> destino) => salientes.ContainsKey(destino);
    }

    public sealed record RutaEncontrada<T>(IReadOnlyList<Vertice<T>> Camino, double Distancia) where T : class;

    // Grafo genérico implementado con LISTAS DE ADYACENCIA. Puede ser:
    //   * DIRIGIDO: cada arista tiene un sentido. Se usa para los motores alternativos
    //     de inventario y de cola (grafos de precedencia).
    //   * NO DIRIGIDO: cada arista funciona en ambos sentidos. Se usa para el mapa de la
    //     tienda; internamente se guardan como dos aristas dirigidas.

    // Algoritmos incluidos
    //    BFS  — recorrido en anchura (con una cola).
    //    DFS  — recorrido en profundidad (recursivo).
    //    Dijkstra — ruta de menor peso entre dos vértices.
    public class Grafo<T> where T : class
    {
        private readonly List<Vertice<T>> vertices = new();
        private int siguienteId = 1;
        private int cantidadArcos;

        public bool EsDirigido { get; }

        public Grafo(bool dirigido)
        {
            EsDirigido = dirigido;
        }

        public IReadOnlyList<Vertice<T>> Vertices => vertices;
        public int CantidadVertices => vertices.Count;

        // Cantidad de aristas lógicas
        public int CantidadAristas => EsDirigido ? cantidadArcos : cantidadArcos / 2;

        // ---------- Construcción ----------
        public Vertice<T> AgregarVertice(T dato, string? etiqueta = null)
        {
            var vertice = new Vertice<T>(siguienteId++, dato, etiqueta ?? dato.ToString() ?? string.Empty);
            vertices.Add(vertice);
            return vertice;
        }

        // Conecta dos vértices
        public void AgregarArista(Vertice<T> origen, Vertice<T> destino, double peso = 1)
        {
            if (origen == destino)
                throw new ArgumentException("No se permiten aristas de un vértice hacia sí mismo.");

            CrearArco(origen, destino, peso);
            if (!EsDirigido) CrearArco(destino, origen, peso);
        }

        private void CrearArco(Vertice<T> origen, Vertice<T> destino, double peso)
        {
            if (!origen.TieneSaliente(destino)) cantidadArcos++;

            var arista = new Arista<T>(origen, destino, peso);
            origen.AgregarSaliente(arista);
            destino.AgregarEntrante(arista);
        }

        // Elimina el vértice y todas las aristas que entran o salen de él
        public void EliminarVertice(Vertice<T> vertice)
        {
            foreach (var arista in vertice.Salientes) arista.Destino.QuitarEntrante(vertice);
            foreach (var arista in vertice.Entrantes) arista.Origen.QuitarSaliente(vertice);

            cantidadArcos -= vertice.GradoSalida + vertice.GradoEntrada;
            vertices.Remove(vertice);
        }

        public void Vaciar()
        {
            vertices.Clear();
            cantidadArcos = 0;
            siguienteId = 1;
        }

        // ---------- BFS ----------
        // Búsqueda en anchura (BFS) del primer vértice cuyo dato cumpla el criterio.
        // Explora en "olas": primero el vértice inicial, luego sus vecinos, luego los vecinos de
        // estos, etc., usando una COLA.
        public Vertice<T>? BuscarBFS(Func<T, bool> criterio)
        {
            var visitados = new HashSet<Vertice<T>>();
            var cola = new Queue<Vertice<T>>();

            foreach (var inicio in vertices)
            {
                if (!visitados.Add(inicio)) continue;
                if (criterio(inicio.Dato)) return inicio;
                cola.Enqueue(inicio);

                while (cola.Count > 0)
                {
                    var actual = cola.Dequeue();

                    foreach (var arista in actual.Salientes)
                    {
                        if (!visitados.Add(arista.Destino)) continue;
                        if (criterio(arista.Destino.Dato)) return arista.Destino;
                        cola.Enqueue(arista.Destino);
                    }

                    foreach (var arista in actual.Entrantes)
                    {
                        if (!visitados.Add(arista.Origen)) continue;
                        if (criterio(arista.Origen.Dato)) return arista.Origen;
                        cola.Enqueue(arista.Origen);
                    }
                }
            }
            return null;
        }

        // Recorrido en anchura desde un vértice
        public List<(Vertice<T> Vertice, int Nivel)> RecorridoBFS(Vertice<T> inicio)
        {
            var resultado = new List<(Vertice<T>, int)>();
            var nivel = new Dictionary<Vertice<T>, int> { [inicio] = 0 };
            var cola = new Queue<Vertice<T>>();
            cola.Enqueue(inicio);

            while (cola.Count > 0)
            {
                var actual = cola.Dequeue();
                resultado.Add((actual, nivel[actual]));

                foreach (var arista in actual.Salientes)
                {
                    if (nivel.ContainsKey(arista.Destino)) continue;
                    nivel[arista.Destino] = nivel[actual] + 1;
                    cola.Enqueue(arista.Destino);
                }
            }
            return resultado;
        }

        // ---------- DFS ----------
        // Recorrido en profundidad (DFS) desde un vértice: avanza por un camino hasta el fondo
        // y solo entonces retrocede (backtracking) para probar otra rama. Es recursivo, por lo
        // que la pila de llamadas hace el papel de la pila explícita.
        public List<Vertice<T>> RecorridoDFS(Vertice<T> inicio)
        {
            var visitados = new HashSet<Vertice<T>>();
            var orden = new List<Vertice<T>>();
            VisitarDFS(inicio, visitados, orden);
            return orden;
        }

        private static void VisitarDFS(Vertice<T> actual, HashSet<Vertice<T>> visitados, List<Vertice<T>> orden)
        {
            visitados.Add(actual);
            orden.Add(actual);

            foreach (var arista in actual.Salientes)
            {
                if (!visitados.Contains(arista.Destino))
                    VisitarDFS(arista.Destino, visitados, orden);
            }
        }

        public List<Vertice<T>>? OrdenTopologico()
        {
            var gradoEntrada = new Dictionary<Vertice<T>, int>(vertices.Count);
            var listos = new Queue<Vertice<T>>();

            foreach (var vertice in vertices)
            {
                gradoEntrada[vertice] = vertice.GradoEntrada;
                if (vertice.GradoEntrada == 0) listos.Enqueue(vertice);
            }

            var orden = new List<Vertice<T>>(vertices.Count);
            while (listos.Count > 0)
            {
                var actual = listos.Dequeue();
                orden.Add(actual);

                foreach (var arista in actual.Salientes)
                {
                    if (--gradoEntrada[arista.Destino] == 0)
                        listos.Enqueue(arista.Destino);
                }
            }

            return orden.Count == vertices.Count ? orden : null;
        }

        // ---------- DIJKSTRA ----------
        // Algoritmo de DIJKSTRA: encuentra la ruta de menor peso total entre dos vértices

        // Idea: se mantiene la mejor distancia conocida a cada vértice (al inicio: 0 para el origen
        // e infinito para los demás). En cada paso se "fija" el vértice pendiente más cercano y se
        // RELAJAN sus aristas: si pasar por él mejora la distancia de un vecino, se actualiza.
        public RutaEncontrada<T>? RutaMasCorta(Vertice<T> origen, Vertice<T> destino, List<string>? traza = null)
        {
            var distancia = new Dictionary<Vertice<T>, double>();
            var anterior = new Dictionary<Vertice<T>, Vertice<T>>();
            var pendientes = new List<Vertice<T>>(vertices);

            foreach (var v in vertices) distancia[v] = double.PositiveInfinity;
            distancia[origen] = 0;

            while (pendientes.Count > 0)
            {
                // Elegir el pendiente con menor distancia conocida.
                Vertice<T> actual = pendientes[0];
                foreach (var candidato in pendientes)
                    if (distancia[candidato] < distancia[actual]) actual = candidato;

                if (double.IsPositiveInfinity(distancia[actual])) break; // El resto no es alcanzable.

                pendientes.Remove(actual);
                traza?.Add($"Se fija '{actual.Etiqueta}' con distancia {Num(distancia[actual])}.");

                if (actual == destino) break; // Ya se conoce la mejor ruta al destino.

                // Relajar las aristas salientes.
                foreach (var arista in actual.Salientes)
                {
                    var vecino = arista.Destino;
                    if (!pendientes.Contains(vecino)) continue; // ya fijado

                    double alternativa = distancia[actual] + arista.Peso;
                    if (alternativa < distancia[vecino])
                    {
                        string antes = double.IsPositiveInfinity(distancia[vecino]) ? "∞" : Num(distancia[vecino]);
                        traza?.Add($"    {vecino.Etiqueta}: {Num(distancia[actual])} + {Num(arista.Peso)} = {Num(alternativa)} (antes: {antes}) → se actualiza.");
                        distancia[vecino] = alternativa;
                        anterior[vecino] = actual;
                    }
                    else
                    {
                        traza?.Add($"    {vecino.Etiqueta}: {Num(distancia[actual])} + {Num(arista.Peso)} = {Num(alternativa)} (no mejora {Num(distancia[vecino])}).");
                    }
                }
            }

            if (double.IsPositiveInfinity(distancia[destino])) return null;

            // Reconstruir el camino recorriendo los "anteriores" desde el destino hacia el origen.
            var camino = new List<Vertice<T>>();
            for (var v = destino; ; v = anterior[v])
            {
                camino.Add(v);
                if (v == origen) break;
            }
            camino.Reverse();

            return new RutaEncontrada<T>(camino, distancia[destino]);
        }

        private static string Num(double valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
