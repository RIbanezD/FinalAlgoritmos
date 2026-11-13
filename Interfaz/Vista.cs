using DonJuanMarket_Ruben_Ibañez.Estructuras;
using DonJuanMarket_Ruben_Ibañez.Modelos;
using DonJuanMarket_Ruben_Ibañez.Servicios;
using DonJuanMarket_Ruben_Ibañez.Utilidades;
using Spectre.Console;

namespace DonJuanMarket_Ruben_Ibañez.Interfaz
{
    // Componentes visuales reutilizables
    public static class Vista
    {
        public static bool DetalleTecnicoActivo { get; set; } = true;

        // Categorías sugeridas con su correlativo numérico, para que el usuario no tenga que
        // escribir el nombre completo al registrar o buscar productos.
        public static readonly (int Numero, string Nombre)[] CategoriasSugeridas =
        {
            (1, "Alimentos"),
            (2, "Bebidas"),
            (3, "Panadería"),
            (4, "Limpieza"),
            (5, "Hogar"),
            (6, "Electrónica"),
            (7, "Otros"),
        };

        // Dibuja la tabla de categorías sugeridas
        public static void MostrarTablaCategorias()
        {
            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            tabla.Title = new TableTitle("[bold]Categorías sugeridas[/]");
            tabla.AddColumn("N.º");
            tabla.AddColumn("Categoría");
            foreach (var (numero, nombre) in CategoriasSugeridas)
                tabla.AddRow($"[cyan]{numero}[/]", Markup.Escape(nombre));
            AnsiConsole.Write(tabla);
            AnsiConsole.MarkupLine("[grey]Puede escribir el número de la lista o el nombre de una categoría nueva.[/]");
        }

        // Convierte lo que el usuario escribió en el nombre de categoría final
        public static string ResolverCategoria(string entrada)
        {
            if (int.TryParse(entrada, out int numero) && numero >= 1 && numero <= CategoriasSugeridas.Length)
                return CategoriasSugeridas[numero - 1].Nombre;
            return entrada;
        }

        // ---------- Estructura general de pantalla ----------
        public static void Limpiar()
        {
            try { AnsiConsole.Clear(); }
            catch (IOException) {}
        }

        public static void Encabezado(Supermercado tienda)
        {
            Limpiar();
            AnsiConsole.Write(new Rule("[bold cyan]DON JUAN MARKET - SISTEMA DE GESTIÓN DE SUPERMERCADO[/]")
            {
                Justification = Justify.Center,
                Style = Style.Parse("cyan")
            });

            AnsiConsole.WriteLine();
        }

        // Títulos
        public static void Titulo(string texto)
        {
            AnsiConsole.Write(new Rule($"[yellow]{Markup.Escape(texto)}[/]").LeftJustified());
            AnsiConsole.MarkupLine($"[grey]Escriba [bold]{Validaciones.TokenVolver}[/] en cualquier momento para cancelar y volver al menú.[/]");
            AnsiConsole.WriteLine();
        }
        public static void TituloSimple(string texto)
        {
            AnsiConsole.Write(new Rule($"[yellow]{Markup.Escape(texto)}[/]").LeftJustified());
            AnsiConsole.WriteLine();
        }

        public static Panel PanelMenu(string contenido, string titulo) => new(contenido)
        {
            Header = new PanelHeader($" {titulo} "),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan"),
            Padding = new Padding(2, 0)
        };

        // ---------- Mensajes ----------
        public static void Exito(string mensaje) => AnsiConsole.MarkupLine($"[bold green]{Markup.Escape(mensaje)}[/]");
        public static void Error(string mensaje) => AnsiConsole.MarkupLine($"[bold red]Error: {Markup.Escape(mensaje)}[/]");
        public static void Aviso(string mensaje) => AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(mensaje)}[/]");
        public static void Info(string mensaje) => AnsiConsole.MarkupLine($"[cyan]{Markup.Escape(mensaje)}[/]");
        public static void Detalle(string mensaje) => AnsiConsole.MarkupLine($"[grey]{Markup.Escape(mensaje)}[/]");

        public static void DetalleTecnico(IReadOnlyList<string> lineas)
        {
            if (!DetalleTecnicoActivo || lineas.Count == 0) return;

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Detalle técnico de la estructura:[/]");
            foreach (string linea in lineas)
                AnsiConsole.MarkupLine($"[grey]  » {Markup.Escape(linea)}[/]");
        }

        // ---------- Productos ----------
        // Ficha con todos los datos de un producto
        public static void TarjetaProducto(Producto p, string titulo)
        {
            var datos = new Grid();
            datos.AddColumn(new GridColumn().NoWrap().PadRight(2));
            datos.AddColumn();

            datos.AddRow("[bold]Código[/]", $": [bold cyan]{Markup.Escape(p.Codigo)}[/]");
            datos.AddRow("[bold]Nombre[/]", $": {Markup.Escape(p.Nombre)}");
            datos.AddRow("[bold]Categoría[/]", $": {Markup.Escape(p.Categoria)}");
            datos.AddRow("[bold]Precio[/]", $": {Formato.Moneda(p.Precio)}");
            datos.AddRow("[bold]Stock[/]", $": [{EstiloVisual.MarkupStock(p.Stock)}]{p.Stock}[/]");

            AnsiConsole.Write(new Panel(datos)
            {
                Header = new PanelHeader($" {titulo} "),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("green"),
                Padding = new Padding(2, 1),
                Expand = false
            });
        }

        // Tabla del inventario
        public static void TablaInventario(List<Producto> productos, string titulo = "INVENTARIO DEL SUPERMERCADO")
        {
            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.Title = new TableTitle($"[bold]{Markup.Escape(titulo)}[/]");
            tabla.AddColumn("Código");
            tabla.AddColumn("Nombre");
            tabla.AddColumn("Categoría");
            tabla.AddColumn(new TableColumn("Precio").RightAligned());
            tabla.AddColumn(new TableColumn("Stock").RightAligned());

            foreach (Producto p in productos)
            {
                tabla.AddRow(
                    $"[bold]{Markup.Escape(p.Codigo)}[/]",
                    Markup.Escape(p.Nombre),
                    Markup.Escape(p.Categoria),
                    Formato.Moneda(p.Precio),
                    $"[{EstiloVisual.MarkupStock(p.Stock)}]{p.Stock}[/]");
            }

            AnsiConsole.Write(tabla);
            AnsiConsole.MarkupLine($"[grey]Total de productos: {productos.Count}   -   " +
                                   $"Stock: [red]rojo[/] = agotado, [yellow]amarillo[/] = {EstiloVisual.UmbralStockBajo} o menos.[/]");
        }

        // ---------- Clientes ----------
        public static void TarjetaCliente(Cliente c, string titulo)
        {
            string color = EstiloVisual.MarkupDe(c.Tipo);

            var datos = new Grid();
            datos.AddColumn(new GridColumn().NoWrap().PadRight(2));
            datos.AddColumn();

            datos.AddRow("[bold]Nombre[/]", $": {Markup.Escape(c.Nombre)}");
            datos.AddRow("[bold]Tipo[/]", $": [bold {color}]{c.Tipo.ATexto()}[/]");
            datos.AddRow("[bold]Productos[/]", $": {c.NumeroProductos}");
            datos.AddRow("[bold]Llegada[/]", $": {Formato.Hora(c.HoraLlegada)}");
            if (c.TiempoEspera.HasValue)
                datos.AddRow("[bold]Esperó[/]", $": {Formato.Duracion(c.TiempoEspera.Value)}");

            AnsiConsole.Write(new Panel(datos)
            {
                Header = new PanelHeader($" {titulo} "),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(EstiloVisual.ColorDe(c.Tipo)),
                Padding = new Padding(2, 1),
                Expand = false
            });
        }

        // Fila de espera
        public static void TablaClientesEnEspera(List<Cliente> clientes)
        {
            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.Title = new TableTitle("[bold]=== CLIENTES EN ESPERA ===[/]");
            tabla.AddColumn("Posición");
            tabla.AddColumn("Nombre");
            tabla.AddColumn("Tipo");
            tabla.AddColumn(new TableColumn("Productos").RightAligned());
            tabla.AddColumn("Llegada");

            for (int i = 0; i < clientes.Count; i++)
            {
                Cliente c = clientes[i];
                string color = EstiloVisual.MarkupDe(c.Tipo);
                tabla.AddRow(
                    $"[{color}]{i + 1}[/]",
                    $"[{color}]{Markup.Escape(c.Nombre)}[/]",
                    $"[bold {color}]{c.Tipo.ATexto()}[/]",
                    $"[{color}]{c.NumeroProductos}[/]",
                    $"[grey]{Formato.Hora(c.HoraLlegada)}[/]");
            }

            AnsiConsole.Write(tabla);
            AnsiConsole.MarkupLine($"[grey]Total en espera: {clientes.Count}[/]");
        }

        // Tabla con los tres niveles de prioridad
        public static void LeyendaTipos()
        {
            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            tabla.Title = new TableTitle("[bold]Niveles de prioridad[/]");
            tabla.AddColumn("Opción");
            tabla.AddColumn("Tipo");
            tabla.AddColumn("Descripción");

            foreach (TipoCliente tipo in Enum.GetValues<TipoCliente>())
            {
                string color = EstiloVisual.MarkupDe(tipo);
                tabla.AddRow($"[bold {color}]{(int)tipo}[/]", $"[bold {color}]{tipo.ATexto()}[/]", tipo.Descripcion());
            }
            AnsiConsole.Write(tabla);
        }

        // ---------- Dibujo de las estructuras ----------

        // dibuja el Árbol AVL
        public static void DibujarArbolAVL(ArbolAVL arbol, int niveles = 5)
        {
            if (arbol.Raiz == null)
            {
                Aviso("El árbol AVL está vacío.");
                return;
            }

            var dibujo = new Tree(new Markup(EtiquetaNodoAVL(arbol.Raiz, "raíz"))) { Style = Style.Parse("cyan") };
            AgregarHijosAVL(dibujo, arbol.Raiz, 1, niveles);
            AnsiConsole.Write(dibujo);

            AnsiConsole.MarkupLine($"[grey]h = altura del nodo - FB = factor de balance (altura izq. − altura der.) - " +
                                   $"Altura del árbol: {arbol.Altura} - Nodos: {arbol.Cantidad}[/]");
            if (arbol.Altura > niveles)
                AnsiConsole.MarkupLine($"[grey]Se muestran solo los primeros {niveles} niveles.[/]");
        }

        private static string EtiquetaNodoAVL(NodoAVL nodo, string rol)
        {
            int fb = nodo.FactorBalance;
            string colorFb = fb == 0 ? "green" : "yellow";
            return $"[grey]{rol}:[/] [bold]{Markup.Escape(nodo.Producto.Codigo)}[/] " +
                   $"[grey]{Markup.Escape(Formato.Truncar(nodo.Producto.Nombre, 22))} - h={nodo.Altura} -[/] [{colorFb}]FB={fb:+0;-0;0}[/]";
        }

        private static void AgregarHijosAVL(IHasTreeNodes padre, NodoAVL nodo, int nivel, int niveles)
        {
            foreach (var (hijo, rol) in new[] { (nodo.Izquierdo, "izq"), (nodo.Derecho, "der") })
            {
                if (hijo == null) continue;
                if (nivel >= niveles)
                {
                    padre.AddNode($"[grey]… ({rol}: hay más niveles)[/]");
                    continue;
                }
                TreeNode rama = padre.AddNode(EtiquetaNodoAVL(hijo, rol));
                AgregarHijosAVL(rama, hijo, nivel + 1, niveles);
            }
        }

        // Dibuja el Heap como árbol
        public static void DibujarHeap(ColaPrioridad cola, int niveles = 5)
        {
            IReadOnlyList<Cliente> arreglo = cola.ObtenerArreglo();
            if (arreglo.Count == 0)
            {
                Aviso("La cola de prioridad está vacía.");
                return;
            }

            AnsiConsole.MarkupLine("[bold]Vista como árbol binario completo[/] [grey](la raíz es el siguiente cliente a atender)[/]");
            var dibujo = new Tree(new Markup(EtiquetaNodoHeap(arreglo, 0))) { Style = Style.Parse("cyan") };
            AgregarHijosHeap(dibujo, arreglo, 0, 1, niveles);
            AnsiConsole.Write(dibujo);
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[bold]Vista como arreglo");
            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            tabla.AddColumn("Índice");
            tabla.AddColumn("Cliente");
            tabla.AddColumn("Tipo");
            tabla.AddColumn("Padre");
            for (int i = 0; i < arreglo.Count; i++)
            {
                string color = EstiloVisual.MarkupDe(arreglo[i].Tipo);
                tabla.AddRow($"{i}", $"[{color}]{Markup.Escape(arreglo[i].Nombre)}[/]",
                             $"[{color}]{arreglo[i].Tipo.ATexto()}[/]", i == 0 ? "[grey]— (raíz)[/]" : $"{(i - 1) / 2}");
            }
            AnsiConsole.Write(tabla);
        }

        private static string EtiquetaNodoHeap(IReadOnlyList<Cliente> arreglo, int indice)
        {
            Cliente c = arreglo[indice];
            string color = EstiloVisual.MarkupDe(c.Tipo);
            return $"[{color}]{Markup.Escape(c.Nombre)} - {c.Tipo.ATexto()}[/] [grey](índice {indice})[/]";
        }

        private static void AgregarHijosHeap(IHasTreeNodes padre, IReadOnlyList<Cliente> arreglo, int indice, int nivel, int niveles)
        {
            foreach (int hijo in new[] { 2 * indice + 1, 2 * indice + 2 })
            {
                if (hijo >= arreglo.Count) continue;
                if (nivel >= niveles)
                {
                    padre.AddNode("[grey]… (hay más niveles)[/]");
                    continue;
                }
                TreeNode rama = padre.AddNode(EtiquetaNodoHeap(arreglo, hijo));
                AgregarHijosHeap(rama, arreglo, hijo, nivel + 1, niveles);
            }
        }

        // Muestra el grafo de precedencia del inventario: orden topológico con los grados de cada vértice
        public static void DibujarGrafoInventario(GrafoInventario inventario, int maximoFilas = 25)
        {
            var orden = inventario.Grafo.OrdenTopologico();
            if (orden == null || orden.Count == 0)
            {
                Aviso("El grafo de inventario está vacío.");
                return;
            }

            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.Title = new TableTitle("[bold]Vértices en orden topológico[/]");
            tabla.AddColumn("Orden");
            tabla.AddColumn("Vértice");
            tabla.AddColumn("Producto");
            tabla.AddColumn(new TableColumn("Aristas entrantes").RightAligned());
            tabla.AddColumn(new TableColumn("Aristas salientes").RightAligned());

            for (int i = 0; i < Math.Min(orden.Count, maximoFilas); i++)
            {
                var v = orden[i];
                tabla.AddRow($"{i + 1}", $"[bold]{Markup.Escape(v.Etiqueta)}[/]", Markup.Escape(Formato.Truncar(v.Dato.Nombre, 26)),
                             $"{v.GradoEntrada}", $"{v.GradoSalida}");
            }
            AnsiConsole.Write(tabla);
            if (orden.Count > maximoFilas)
                AnsiConsole.MarkupLine($"[grey]Se muestran {maximoFilas} de {orden.Count} vértices.[/]");

            AnsiConsole.MarkupLine($"[bold]Vértices:[/] {inventario.Grafo.CantidadVertices}   [bold]Aristas:[/] {inventario.Grafo.CantidadAristas}   " +
                                   "[grey](un par por cada dos productos: n-(n−1)/2)[/]");
            AnsiConsole.MarkupLine("[grey]Observe: las aristas ENTRANTES de un vértice = cuántos productos tienen un código menor = su posición − 1.[/]");
            AnsiConsole.MarkupLine("[bold]Orden:[/] " + string.Join(" [grey]→[/] ", orden.Take(10).Select(v => Markup.Escape(v.Etiqueta))) +
                                   (orden.Count > 10 ? " [grey]→ …[/]" : string.Empty));
        }

        // Muestra el grafo de precedencia de la fila de atención
        public static void DibujarGrafoAtencion(GrafoAtencion cola, int maximoFilas = 25)
        {
            var orden = cola.Grafo.OrdenTopologico();
            if (orden == null || orden.Count == 0)
            {
                Aviso("El grafo de atención está vacío.");
                return;
            }

            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.Title = new TableTitle("[bold]Vértices en orden topológico (orden de atención)[/]");
            tabla.AddColumn("Orden");
            tabla.AddColumn("Cliente");
            tabla.AddColumn("Tipo");
            tabla.AddColumn(new TableColumn("Van antes (entrantes)").RightAligned());
            tabla.AddColumn(new TableColumn("Van después (salientes)").RightAligned());

            for (int i = 0; i < Math.Min(orden.Count, maximoFilas); i++)
            {
                var v = orden[i];
                string color = EstiloVisual.MarkupDe(v.Dato.Tipo);
                tabla.AddRow($"[{color}]{i + 1}[/]", $"[{color}]{Markup.Escape(v.Etiqueta)}[/]", $"[{color}]{v.Dato.Tipo.ATexto()}[/]",
                             $"{v.GradoEntrada}", $"{v.GradoSalida}");
            }
            AnsiConsole.Write(tabla);
            if (orden.Count > maximoFilas)
                AnsiConsole.MarkupLine($"[grey]Se muestran {maximoFilas} de {orden.Count} vértices.[/]");

            AnsiConsole.MarkupLine($"[bold]Vértices:[/] {cola.Grafo.CantidadVertices}   [bold]Aristas:[/] {cola.Grafo.CantidadAristas}");
            AnsiConsole.MarkupLine("[grey]El siguiente cliente es el único vértice con 0 aristas entrantes (nadie debe pasar antes que él).[/]");
        }

        // ---------- Mapa de la tienda ----------
        // Dibujo
        public static void DibujarMapaAereo()
        {
            string[] lineas =
            {
                "                     ┌───────────────┐",
                "                     │   PANADERÍA   │",
                "                     └───────┬───────┘",
                "                             │",
                "   ┌───────────────┐ ┌───────┴───────┐ ┌───────────────┐",
                "   │    LIMPIEZA   ├─┤   ALIMENTOS   ├─┤    BEBIDAS    │",
                "   └───────┬───────┘ └───────┬───────┘ └───────┬───────┘",
                "           │                 │                 │",
                "   ┌───────┴───────┐         │         ┌───────┴───────┐",
                "   │      HOGAR    │         │         │  ELECTRÓNICA  │",
                "   └───────┬───────┘         │         └───────┬───────┘",
                "           │                 │                 │",
                "   ┌───────┴───────┐         │                 │",
                "   │      OTROS    │         │                 │",
                "   └───────────────┘         │                 │",
                "                     ┌───────┴───────┐         │",
                "                     │    ENTRADA    │         │",
                "                     │   (acceso)    │         │",
                "                     └───────┬───────┘         │",
                "                             │                 │",
                "                             └────────┬────────┘",
                "                              ┌────────┴────────┐",
                "                              │      CAJAS      │",
                "                              └─────────────────┘",
            };

            var panel = new Panel(Markup.Escape(string.Join("\n", lineas)))
            {
                Header = new PanelHeader(" Vista aérea de la tienda (ilustrativa) "),
                Border = BoxBorder.Rounded,
                BorderStyle = Style.Parse("cyan"),
                Padding = new Padding(2, 1),
                Expand = false,
            };
            AnsiConsole.Write(panel);
            AnsiConsole.MarkupLine("[grey]Las distancias exactas en metros están en la tabla de adyacencia[/]");
        }
    }
}