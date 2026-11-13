using DonJuanMarket_Ruben_Ibañez.Modelos;
using Spectre.Console;

namespace DonJuanMarket_Ruben_Ibañez.Utilidades
{
    public static class EstiloVisual
    {
        // Stock igual o menor a este valor se considera bajo
        public const int UmbralStockBajo = 10;

        public static Color ColorDe(TipoCliente tipo) => tipo switch
        {
            TipoCliente.Vip => Color.Gold1,
            TipoCliente.AdultoMayor => Color.Orange1,
            _ => Color.DeepSkyBlue1
        };

        public static string MarkupDe(TipoCliente tipo) => tipo switch
        {
            TipoCliente.Vip => "gold1",
            TipoCliente.AdultoMayor => "orange1",
            _ => "deepskyblue1"
        };

        // Rojo si no hay stock, amarillo si es bajo, verde si es normal
        public static string MarkupStock(int stock) => stock switch
        {
            0 => "red",
            <= UmbralStockBajo => "yellow",
            _ => "green"
        };
    }
}
