using DonJuanMarket_Ruben_Ibañez.Modelos;

namespace DonJuanMarket_Ruben_Ibañez.Servicios
{
    // Resumen de una categoría del inventario
    public record ResumenCategoria(string Categoria, int Productos, int Unidades, double Valor);

    public class EstadisticasSistema
    {
        public string EstructuraInventario { get; init; } = string.Empty;
        public string EstructuraCola { get; init; } = string.Empty;

        // Inventario
        public int TotalProductos { get; init; }
        public int UnidadesEnStock { get; init; }
        public double ValorInventario { get; init; }
        public int ProductosAgotados { get; init; }
        public int ProductosStockBajo { get; init; }
        public Producto? MasCaro { get; init; }
        public Producto? MayorStock { get; init; }
        public List<ResumenCategoria> Categorias { get; init; } = new();

        // Solo Árbol AVL 
        public int? AlturaArbol { get; init; }
        public string? RaizArbol { get; init; }
        public int? RotacionesSimples { get; init; }
        public int? RotacionesDobles { get; init; }

        // Atención de clientes
        public int ClientesEnEspera { get; init; }
        public int EsperaVip { get; init; }
        public int EsperaAdultoMayor { get; init; }
        public int EsperaRegular { get; init; }
        public int ArticulosPorProcesar { get; init; }
        public Cliente? SiguienteCliente { get; init; }

        public int ClientesAtendidos { get; init; }
        public int AtendidosVip { get; init; }
        public int AtendidosAdultoMayor { get; init; }
        public int AtendidosRegular { get; init; }
        public int ArticulosProcesados { get; init; }
        public TimeSpan? EsperaPromedio { get; init; }
    }
}
