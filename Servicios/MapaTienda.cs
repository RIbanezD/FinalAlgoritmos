using DonJuanMarket_Ruben_Ibañez.Estructuras;
using DonJuanMarket_Ruben_Ibañez.Utilidades;

namespace DonJuanMarket_Ruben_Ibañez.Servicios
{
    //   * Vértice = sección de la tienda (Entrada, Alimentos, Bebidas, Cajas...).
    //   * Arista  = pasillo que conecta dos secciones; su peso son los metros de distancia.
    public class MapaTienda
    {
        public const string Entrada = "Entrada";
        public const string Cajas = "Cajas";
        public const string Otros = "Otros";

        private readonly Grafo<string> grafo = new(dirigido: false);
        private readonly Dictionary<string, Vertice<string>> porNombre = new();

        public Grafo<string> Grafo => grafo;
        public IReadOnlyList<Vertice<string>> Secciones => grafo.Vertices;

        public MapaTienda()
        {
            foreach (string nombre in new[] { Entrada, "Panadería", "Alimentos", "Bebidas", "Limpieza", "Hogar", "Electrónica", Otros, Cajas })
                porNombre[Formato.Normalizar(nombre)] = grafo.AgregarVertice(nombre, nombre);

            // Pasillos (distancias en metros).
            Conectar(Entrada, "Panadería", 6);
            Conectar(Entrada, "Alimentos", 10);
            Conectar(Entrada, "Bebidas", 16);
            Conectar("Panadería", "Alimentos", 5);
            Conectar("Alimentos", "Bebidas", 4);
            Conectar("Alimentos", "Limpieza", 9);
            Conectar("Limpieza", "Hogar", 6);
            Conectar("Hogar", "Electrónica", 7);
            Conectar("Hogar", Otros, 5);
            Conectar("Bebidas", "Electrónica", 12);
            Conectar("Bebidas", Cajas, 10);
            Conectar("Electrónica", Cajas, 8);
            Conectar("Limpieza", Cajas, 11);
        }

        private void Conectar(string a, string b, double metros) =>
            grafo.AgregarArista(ObtenerSeccion(a), ObtenerSeccion(b), metros);

        // Devuelve el vértice de una sección por su nombre
        public Vertice<string> ObtenerSeccion(string nombre) =>
            porNombre[Formato.Normalizar(nombre)];

        // Sección donde se encuentra una categoría de producto ("Otros" si no tiene una propia)
        public Vertice<string> SeccionParaCategoria(string categoria) =>
            porNombre.TryGetValue(Formato.Normalizar(categoria), out var seccion)
                ? seccion
                : porNombre[Formato.Normalizar(Otros)];
    }
}
