using DonJuanMarket_Ruben_Ibañez.Modelos;

namespace DonJuanMarket_Ruben_Ibañez.Estructuras
{

    public class ColaPrioridad : IColaAtencion
    {
        private const int CapacidadInicial = 16;

        private Cliente[] heap = new Cliente[CapacidadInicial];
        private int cantidad;
        private long comparaciones;
        private readonly List<string> bitacora = new();

        public string Nombre => "Cola de Prioridad (Min Heap)";
        public string Etiqueta => "HEAP";
        public string ResumenComplejidad => "Agregar y extraer: O(log n) · Consultar siguiente: O(1)";

        public int Cantidad => cantidad;
        public bool EstaVacia => cantidad == 0;
        public long Comparaciones => comparaciones;

        public IReadOnlyList<string> UltimaOperacion => bitacora;

        // ---------- AGREGAR ----------
        // agregar cliente a cola
        public void Agregar(Cliente cliente)
        {
            ArgumentNullException.ThrowIfNull(cliente);
            bitacora.Clear();

            if (cantidad == heap.Length)
                Array.Resize(ref heap, heap.Length * 2);

            int indiceInicial = cantidad;
            heap[cantidad] = cliente;
            cantidad++;

            int indiceFinal = HeapUp(indiceInicial);

            bitacora.Add(indiceFinal == indiceInicial
                ? $"Bubble up: {cliente.Nombre} se colocó en el índice {indiceInicial} y no necesitó subir (su padre tiene mayor o igual prioridad)."
                : $"Bubble up: {cliente.Nombre} subió del índice {indiceInicial} al {indiceFinal} ({NumeroIntercambios(indiceInicial, indiceFinal)} intercambio(s)).");
        }

        // Heap-up
        private int HeapUp(int indice)
        {
            while (indice > 0)
            {
                int padre = (indice - 1) / 2;

                if (!Precede(heap[indice], heap[padre]))
                    break;

                Intercambiar(indice, padre);
                indice = padre;
            }
            return indice;
        }

        // ---------- EXTRAER ----------
        // cliente mayor prioridad
        public Cliente? Extraer()
        {
            bitacora.Clear();
            if (cantidad == 0) return null;

            Cliente siguiente = heap[0];

            cantidad--;
            heap[0] = heap[cantidad];
            heap[cantidad] = null!;

            int indiceFinal = 0;
            if (cantidad > 0)
                indiceFinal = HeapDown(0);

            bitacora.Add(cantidad == 0
                ? $"Bubble down: no fue necesario, la cola quedó vacía tras extraer a {siguiente.Nombre}."
                : $"Bubble down: el último elemento pasó a la raíz y bajó hasta el índice {indiceFinal} ({NumeroIntercambios(0, indiceFinal)} intercambio(s)).");

            return siguiente;
        }

        // Heap-Down
        private int HeapDown(int indice)
        {
            while (true)
            {
                int izquierdo = 2 * indice + 1;
                int derecho = izquierdo + 1;
                int mejor = indice;

                if (izquierdo < cantidad && Precede(heap[izquierdo], heap[mejor]))
                    mejor = izquierdo;

                if (derecho < cantidad && Precede(heap[derecho], heap[mejor]))
                    mejor = derecho;

                if (mejor == indice)
                    break;

                Intercambiar(indice, mejor);
                indice = mejor;
            }
            return indice;
        }

        // ---------- CONSULTAS ----------
        // peek siguiente cliente
        public Cliente? VerSiguiente() => cantidad == 0 ? null : heap[0];

        // posicion fila del cliente
        public int PosicionEstimada(Cliente cliente)
        {
            int antes = 0;
            for (int i = 0; i < cantidad; i++)
            {
                if (heap[i].TienePrioridadSobre(cliente)) antes++;
            }
            return antes + 1;
        }

        // lista orden clientes
        public List<Cliente> ObtenerEnOrdenDeAtencion()
        {
            var copia = new ColaPrioridad
            {
                heap = (Cliente[])heap.Clone(),
                cantidad = cantidad
            };

            var resultado = new List<Cliente>(cantidad);
            Cliente? siguiente;
            while ((siguiente = copia.Extraer()) != null)
                resultado.Add(siguiente);

            return resultado;
        }

        // arreglo sin ordenamiento
        public IReadOnlyList<Cliente> ObtenerArreglo() => heap.Take(cantidad).ToList();

        // ---------- APOYO ----------
        private bool Precede(Cliente a, Cliente b)
        {
            comparaciones++;
            return a.TienePrioridadSobre(b);
        }

        private void Intercambiar(int i, int j) => (heap[i], heap[j]) = (heap[j], heap[i]);

        // numero de intercambio
        private static int NumeroIntercambios(int desde, int hasta)
        {
            int saltos = 0;
            int mayor = Math.Max(desde, hasta);
            int menor = Math.Min(desde, hasta);
            while (mayor > menor)
            {
                mayor = (mayor - 1) / 2;
                saltos++;
            }
            return saltos;
        }

        public void ReiniciarContadores() => comparaciones = 0;

        public void Vaciar()
        {
            heap = new Cliente[CapacidadInicial];
            cantidad = 0;
            bitacora.Clear();
        }

        // validar heap
        public bool VerificarPropiedadHeap()
        {
            for (int i = 1; i < cantidad; i++)
            {
                int padre = (i - 1) / 2;
                if (heap[i].TienePrioridadSobre(heap[padre])) return false;
            }
            return true;
        }
    }
}
