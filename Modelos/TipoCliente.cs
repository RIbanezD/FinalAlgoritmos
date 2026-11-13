namespace DonJuanMarket_Ruben_Ibañez.Modelos
{
    public enum TipoCliente
    {
        Vip = 1,
        AdultoMayor = 2,
        Regular = 3
    }

    // Métodos de apoyo para mostrar y convertir el tipo de cliente
    public static class TipoClienteExtensiones
    {
        public static string ATexto(this TipoCliente tipo) => tipo switch
        {
            TipoCliente.Vip => "VIP",
            TipoCliente.AdultoMayor => "ADULTO_MAYOR",
            TipoCliente.Regular => "REGULAR",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };

        public static string Descripcion(this TipoCliente tipo) => tipo switch
        {
            TipoCliente.Vip => "Clientes premium",
            TipoCliente.AdultoMayor => "Adultos mayores de 65 años",
            TipoCliente.Regular => "Clientes estándar",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };

        public static bool TryConvertir(string? texto, out TipoCliente tipo)
        {
            tipo = TipoCliente.Regular;
            if (string.IsNullOrWhiteSpace(texto)) return false;

            switch (texto.Trim().ToUpperInvariant().Replace(' ', '_'))
            {
                case "1": case "VIP":
                    tipo = TipoCliente.Vip; return true;
                case "2": case "ADULTO_MAYOR": case "ADULTOMAYOR":
                    tipo = TipoCliente.AdultoMayor; return true;
                case "3": case "REGULAR":
                    tipo = TipoCliente.Regular; return true;
                default:
                    return false;
            }
        }
    }
}
