using DonJuanMarket_Ruben_Ibañez.Interfaz;
using DonJuanMarket_Ruben_Ibañez.Servicios;

namespace DonJuanMarket_Ruben_Ibañez
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.InputEncoding = System.Text.Encoding.UTF8;
            }
            catch (IOException){}

            var tienda = new Supermercado();
            var menu = new Menu(tienda);
            menu.Ejecutar();
        }
    }
}
