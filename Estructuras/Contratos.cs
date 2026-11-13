using DonJuanMarket_Ruben_Ibañez.Modelos;

namespace DonJuanMarket_Ruben_Ibañez.Estructuras
{
    public interface IInventario
    {
        // Nombre completo de la estructura (para pantallas informativas
        string Nombre { get; }

        // Cantidad de productos almacenados
        int Cantidad { get; }

        // true si se insertó; false si el código ya existía
        bool Insertar(Producto producto);

        // El producto, o null si no existe. No modifica la estructura
        Producto? Buscar(string codigo);

        // true si el producto existía y fue actualizado; false si no existe
        bool Actualizar(string codigo, string nombre, string categoria, double precio, int stock);

        // true si el producto existía y fue eliminado; false si no existe
        bool Eliminar(string codigo);

        // Productos ordenados ascendentemente por código
        List<Producto> ObtenerOrdenado();

        void ReiniciarContadores();

        void Vaciar();
    }

    public interface IColaAtencion
    {
        string Nombre { get; }

        int Cantidad { get; }

        // Inserta al cliente en la posición que le corresponde según su prioridad
        void Agregar(Cliente cliente);

        // Retira y devuelve al siguiente cliente a atender (null si la cola está vacía
        Cliente? Extraer();

        // Consulta al siguiente cliente sin retirarlo
        Cliente? VerSiguiente();

        // posición (1 = próximo en ser atendido) que ocupa el cliente en la fila
        int PosicionEstimada(Cliente cliente);

        // Lista completa en orden de atención, sin alterar la cola original
        List<Cliente> ObtenerEnOrdenDeAtencion();

        void ReiniciarContadores();

        void Vaciar();
    }
}
