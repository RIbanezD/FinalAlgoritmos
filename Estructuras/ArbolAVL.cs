using DonJuanMarket_Ruben_Ibañez.Modelos;

namespace DonJuanMarket_Ruben_Ibañez.Estructuras
{
    public class ArbolAVL : IInventario
    {
        public sealed record RegistroRotacion(int Numero, string Tipo, string NodoPivote, int FactorBalance, string Motivo, DateTime Momento);

        private NodoAVL? raiz;
        private long comparaciones;
        private readonly List<string> bitacora = new();
        private readonly List<RegistroRotacion> historialRotaciones = new();
        private int contadorRotaciones;
        private string operacionActual = "—";

        // ---------- Propiedades ----------

        public string Nombre => "Árbol AVL";
        public string Etiqueta => "AVL";
        public string ResumenComplejidad => "Insertar, buscar y eliminar: O(log n) · Recorrido in-orden: O(n)";

        // Cantidad de productos almacenados
        public int Cantidad { get; private set; }

        // Raíz del árbol
        public NodoAVL? Raiz => raiz;

        // Altura total del árbol (árbol vacío = 0)
        public int Altura => NodoAVL.AlturaDe(raiz);

        // Rotaciones simples aplicadas desde que se creó el árbol
        public int RotacionesSimples { get; private set; }

        // Rotaciones dobles aplicadas desde que se creó el árbol
        public int RotacionesDobles { get; private set; }

        public long Comparaciones => comparaciones;

        // Mensajes sobre el rebalanceo realizado en la última inserción o eliminación
        public IReadOnlyList<string> UltimaOperacion => bitacora;

        // Historial rotaciones
        public IReadOnlyList<RegistroRotacion> HistorialRotaciones => historialRotaciones;

        // ---------- INSERTAR ----------
        // producto a insertar
        public bool Insertar(Producto producto)
        {
            ArgumentNullException.ThrowIfNull(producto);
            bitacora.Clear();
            operacionActual = $"Insertar {producto.Codigo}";

            bool insertado = false;
            raiz = Insertar(raiz, producto, ref insertado);

            if (insertado)
            {
                Cantidad++;
                if (bitacora.Count == 0)
                    bitacora.Add("Sin rotaciones: el árbol se mantuvo balanceado.");
            }
            return insertado;
        }

        // Insercion nodo
        private NodoAVL Insertar(NodoAVL? nodo, Producto producto, ref bool insertado)
        {
            if (nodo == null)
            {
                insertado = true;
                return new NodoAVL(producto);
            }

            comparaciones++;
            int comparacion = string.CompareOrdinal(producto.Codigo, nodo.Producto.Codigo);

            if (comparacion < 0)
                nodo.Izquierdo = Insertar(nodo.Izquierdo, producto, ref insertado);
            else if (comparacion > 0)
                nodo.Derecho = Insertar(nodo.Derecho, producto, ref insertado);
            else
                return nodo; // Código duplicado

            return insertado ? Rebalancear(nodo) : nodo;
        }

        // ---------- BUSCAR ----------
        // Buscar producto
        public Producto? Buscar(string codigo)
        {
            NodoAVL? actual = raiz;
            while (actual != null)
            {
                comparaciones++;
                int comparacion = string.CompareOrdinal(codigo, actual.Producto.Codigo);
                if (comparacion == 0) return actual.Producto;
                actual = comparacion < 0 ? actual.Izquierdo : actual.Derecho;
            }
            return null;
        }

        // ---------- ACTUALIZAR ----------
        // actualizar producto
        public bool Actualizar(string codigo, string nombre, string categoria, double precio, int stock)
        {
            bitacora.Clear();
            Producto? producto = Buscar(codigo);
            if (producto == null) return false;

            producto.Actualizar(nombre, categoria, precio, stock);
            bitacora.Add("La clave (código) no cambió: no se requiere rebalanceo.");
            return true;
        }

        // ---------- ELIMINAR ----------
        // eliminar producto
        public bool Eliminar(string codigo)
        {
            bitacora.Clear();
            operacionActual = $"Eliminar {codigo}";

            bool eliminado = false;
            raiz = Eliminar(raiz, codigo, ref eliminado);

            if (eliminado)
            {
                Cantidad--;
                if (bitacora.Count == 0)
                    bitacora.Add("Sin rotaciones: el árbol se mantuvo balanceado.");
            }
            return eliminado;
        }

        private NodoAVL? Eliminar(NodoAVL? nodo, string codigo, ref bool eliminado)
        {
            if (nodo == null) return null; // El código no existe.

            comparaciones++;
            int comparacion = string.CompareOrdinal(codigo, nodo.Producto.Codigo);

            if (comparacion < 0)
            {
                nodo.Izquierdo = Eliminar(nodo.Izquierdo, codigo, ref eliminado);
            }
            else if (comparacion > 0)
            {
                nodo.Derecho = Eliminar(nodo.Derecho, codigo, ref eliminado);
            }
            else
            {
                eliminado = true;

                if (nodo.Izquierdo == null) return nodo.Derecho;
                if (nodo.Derecho == null) return nodo.Izquierdo;

                // se reemplaza por su sucer inorden
                NodoAVL sucesor = MinimoDe(nodo.Derecho);
                bitacora.Add($"Nodo con dos hijos: se reemplazó por su sucesor in-orden {sucesor.Producto.Codigo}.");
                nodo.Producto = sucesor.Producto;

                bool auxiliar = false;
                nodo.Derecho = Eliminar(nodo.Derecho, sucesor.Producto.Codigo, ref auxiliar);
            }

            return eliminado ? Rebalancear(nodo) : nodo;
        }

        private static NodoAVL MinimoDe(NodoAVL nodo)
        {
            while (nodo.Izquierdo != null) nodo = nodo.Izquierdo;
            return nodo;
        }

        // ---------- RECORRIDO INORDEN ----------
        // recorrido inorden
        public List<Producto> ObtenerOrdenado()
        {
            var resultado = new List<Producto>(Cantidad);
            RecorridoInOrden(raiz, resultado);
            return resultado;
        }

        private static void RecorridoInOrden(NodoAVL? nodo, List<Producto> resultado)
        {
            if (nodo == null) return;                     
            RecorridoInOrden(nodo.Izquierdo, resultado);  // todo lo menor
            resultado.Add(nodo.Producto);                 // el propio nodo
            RecorridoInOrden(nodo.Derecho, resultado);    // todo lo mayor
        }

        // ---------- REBALANCEO ----------
        // Rebalanceo del arbol
        private NodoAVL Rebalancear(NodoAVL nodo)
        {
            nodo.ActualizarAltura();
            int balance = nodo.FactorBalance;

            // desbalance izq
            if (balance > 1)
            {
                if (nodo.Izquierdo!.FactorBalance >= 0)
                {
                    RotacionesSimples++;
                    bitacora.Add($"Rotación simple a la derecha (caso Izquierda-Izquierda) sobre {nodo.Producto.Codigo} (FB = {balance:+0;-0;0}).");
                    RegistrarRotacion("LL — simple a la derecha", nodo.Producto.Codigo, balance);
                    return RotarDerecha(nodo);
                }

                RotacionesDobles++;
                bitacora.Add($"Rotación doble izquierda-derecha (caso Izquierda-Derecha) sobre {nodo.Producto.Codigo} (FB = {balance:+0;-0;0}).");
                RegistrarRotacion("LR — doble (izquierda y luego derecha)", nodo.Producto.Codigo, balance);
                nodo.Izquierdo = RotarIzquierda(nodo.Izquierdo);
                return RotarDerecha(nodo);
            }

            // desbalance der
            if (balance < -1)
            {
                if (nodo.Derecho!.FactorBalance <= 0)
                {
                    RotacionesSimples++;
                    bitacora.Add($"Rotación simple a la izquierda (caso Derecha-Derecha) sobre {nodo.Producto.Codigo} (FB = {balance:+0;-0;0}).");
                    RegistrarRotacion("RR — simple a la izquierda", nodo.Producto.Codigo, balance);
                    return RotarIzquierda(nodo);
                }

                RotacionesDobles++;
                bitacora.Add($"Rotación doble derecha-izquierda (caso Derecha-Izquierda) sobre {nodo.Producto.Codigo} (FB = {balance:+0;-0;0}).");
                RegistrarRotacion("RL — doble (derecha y luego izquierda)", nodo.Producto.Codigo, balance);
                nodo.Derecho = RotarDerecha(nodo.Derecho);
                return RotarIzquierda(nodo);
            }

            return nodo;
        }

        // agregar fila al historial de rotaciones
        private void RegistrarRotacion(string tipo, string nodoPivote, int factorBalance)
        {
            contadorRotaciones++;
            historialRotaciones.Add(new RegistroRotacion(contadorRotaciones, tipo, nodoPivote, factorBalance, operacionActual, DateTime.Now));
        }

        // Rotacion derecha
        private static NodoAVL RotarDerecha(NodoAVL y)
        {
            NodoAVL x = y.Izquierdo!;
            NodoAVL? t2 = x.Derecho;

            x.Derecho = y;
            y.Izquierdo = t2;

            y.ActualizarAltura();
            x.ActualizarAltura();
            return x;
        }

        // Rotacion izquierda
        private static NodoAVL RotarIzquierda(NodoAVL x)
        {
            NodoAVL y = x.Derecho!;
            NodoAVL? t2 = y.Izquierdo;

            y.Izquierdo = x;
            x.Derecho = t2;

            x.ActualizarAltura();
            y.ActualizarAltura();
            return y;
        }

        // ---------- UTILIDADES ----------
        public void ReiniciarContadores() => comparaciones = 0;

        // vaciar arbol
        public void Vaciar()
        {
            raiz = null;
            Cantidad = 0;
            bitacora.Clear();
            historialRotaciones.Clear();
            contadorRotaciones = 0;
        }
    }
}