using DonJuanMarket_Ruben_Ibañez.Modelos;

namespace DonJuanMarket_Ruben_Ibañez.Estructuras
{
    public class NodoAVL
    {
        // Producto almacenado
        public Producto Producto { get; internal set; }

        // Hijo izquierdo: contiene códigos menores
        public NodoAVL? Izquierdo { get; internal set; }

        // Hijo derecho: contiene códigos mayores
        public NodoAVL? Derecho { get; internal set; }

        // Altura del nodo
        public int Altura { get; internal set; } = 1;

        public NodoAVL(Producto producto)
        {
            Producto = producto ?? throw new ArgumentNullException(nameof(producto));
        }

        // Altura de un nodo que puede ser 0
        public static int AlturaDe(NodoAVL? nodo) => nodo?.Altura ?? 0;

        // Factor de balance
        public int FactorBalance => AlturaDe(Izquierdo) - AlturaDe(Derecho);

        public bool EsHoja => Izquierdo == null && Derecho == null;

        // Recalcula la altura
        internal void ActualizarAltura()
        {
            Altura = 1 + Math.Max(AlturaDe(Izquierdo), AlturaDe(Derecho));
        }
    }
}
