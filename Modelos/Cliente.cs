namespace DonJuanMarket_Ruben_Ibañez.Modelos
{
    public class Cliente
    {
        public string Nombre { get; }
        public TipoCliente Tipo { get; }

        // Cantidad de artículos que el cliente va a comprar
        public int NumeroProductos { get; }

        // Momento en que el cliente entró a la cola
        public DateTime HoraLlegada { get; }

        // Momento en que fue atendido; null mientras siga esperando
        public DateTime? HoraAtencion { get; private set; }

        // Número consecutivo asignado al registrarse 
        public long Turno { get; private set; }

        // Prioridad numérica (1 = máxima)
        public int Prioridad => (int)Tipo;

        public bool Atendido => HoraAtencion.HasValue;

        // Tiempo que esperó en la cola
        public TimeSpan? TiempoEspera => HoraAtencion.HasValue ? HoraAtencion.Value - HoraLlegada : null;

        public Cliente(string nombre, TipoCliente tipo, int numeroProductos, DateTime? horaLlegada = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del cliente no puede estar vacío.", nameof(nombre));
            if (nombre.Trim().Length > 60)
                throw new ArgumentException("El nombre del cliente no puede superar los 60 caracteres.", nameof(nombre));
            if (!Enum.IsDefined(tipo))
                throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de cliente inválido.");
            if (numeroProductos < 1)
                throw new ArgumentOutOfRangeException(nameof(numeroProductos), "El número de productos debe ser al menos 1.");

            Nombre = nombre.Trim();
            Tipo = tipo;
            NumeroProductos = numeroProductos;
            HoraLlegada = horaLlegada ?? DateTime.Now;
        }

        // Asigna el número de turno
        internal void AsignarTurno(long turno)
        {
            if (Turno == 0) Turno = turno;
        }

        // Registra el momento de atención del cliente
        internal void MarcarAtendido(DateTime? cuando = null)
        {
            HoraAtencion = cuando ?? DateTime.Now;
        }

        // Criterio de ordenación de la Cola de Prioridad
        public bool TienePrioridadSobre(Cliente otro)
        {
            if (Prioridad != otro.Prioridad) return Prioridad < otro.Prioridad;

            int porHora = HoraLlegada.CompareTo(otro.HoraLlegada);
            if (porHora != 0) return porHora < 0;

            return Turno < otro.Turno;
        }

        public override string ToString() => $"{Nombre} ({Tipo.ATexto()}, {NumeroProductos} productos)";
    }
}
