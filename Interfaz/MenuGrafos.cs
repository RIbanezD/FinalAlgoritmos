using DonJuanMarket_Ruben_Ibañez.Estructuras;
using DonJuanMarket_Ruben_Ibañez.Modelos;
using DonJuanMarket_Ruben_Ibañez.Servicios;
using DonJuanMarket_Ruben_Ibañez.Utilidades;
using Spectre.Console;

namespace DonJuanMarket_Ruben_Ibañez.Interfaz
{
    public class MenuGrafos
    {
        private readonly Supermercado tienda;

        public MenuGrafos(Supermercado tienda)
        {
            this.tienda = tienda ?? throw new ArgumentNullException(nameof(tienda));
        }

        // ---------- Ciclo del menú ----------
        public void Ejecutar()
        {
            bool volver = false;
            while (!volver)
            {
                Vista.Encabezado(tienda);
                MostrarMenu();

                string opcion = Validaciones.LeerTexto("Seleccione una opción:", permitirVacio: true, permitirVolver: false);
                volver = ProcesarOpcion(opcion);
            }
        }

        private bool ProcesarOpcion(string opcion)
        {
            switch (opcion.ToLowerInvariant())
            {
                case "1": VerEstructuraInventario(); break;
                case "2": VerEstructuraCola(); break;
                case "3": DemostrarRotaciones(); break;
                case "4": VerMapa(); break;
                case "5": RecorridosBfsDfs(); break;
                case "6": RutaMasCorta(); break;
                case "7": case Validaciones.TokenVolver: return true;
                default:
                    Vista.Error("Opción inválida. Elija un número del 1 al 7.");
                    Validaciones.ReturnMenu();
                    break;
            }
            return false;
        }

        private void MostrarMenu()
        {
            AnsiConsole.Write(new Rule("[bold magenta]LABORATORIO DE GRAFOS[/]").Centered().RuleStyle("magenta"));
            AnsiConsole.WriteLine();

            AnsiConsole.Write(Vista.PanelMenu(
                "[bold yellow]MOTOR DE ESTRUCTURAS[/]\n" +
                "[green][[1]][/] Ver la estructura interna del inventario\n" +
                "[green][[2]][/] Ver la estructura interna de la cola de atención\n" +
                "[green][[3]][/] Historial de rotaciones del Árbol AVL\n" +
                "\n" +
                "[bold yellow]ALGORITMOS SOBRE GRAFOS (MAPA DE LA TIENDA)[/]\n" +
                "[green][[4]][/] Mapa de la tienda (lista de adyacencia)\n" +
                "[green][[5]][/] Recorridos BFS y DFS\n" +
                "[green][[6]][/] Ruta más corta a un producto (Dijkstra)\n" +
                "\n" +
                "[green][[7]][/] Volver al menú principal",
                "Laboratorio"));

            AnsiConsole.WriteLine();
        }

        // ---------- Ver estructuras ----------
        private void VerEstructuraInventario()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple($"Estructura interna del inventario - {tienda.Inventario.Nombre}");

            if (tienda.Inventario.Cantidad == 0)
            {
                Vista.Aviso("El inventario está vacío. Cargue datos de demostración desde el menú principal (opción 12).");
            }
            else if (tienda.Inventario is ArbolAVL avl)
            {
                Vista.DibujarArbolAVL(avl);
                AnsiConsole.MarkupLine($"[bold]Rotaciones aplicadas hasta ahora:[/] {avl.RotacionesSimples} simple(s) y {avl.RotacionesDobles} doble(s).");
            }

            Validaciones.ReturnMenu();
        }

        private void VerEstructuraCola()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple($"Estructura interna de la cola - {tienda.ColaClientes.Nombre}");

            if (tienda.ColaClientes.Cantidad == 0)
            {
                Vista.Aviso("No hay clientes en espera. Agregue clientes con la opción 6 del menú principal.");
            }
            else if (tienda.ColaClientes is ColaPrioridad heap)
            {
                Vista.DibujarHeap(heap);
            }
            else if (tienda.ColaClientes is GrafoAtencion grafo)
            {
                Vista.DibujarGrafoAtencion(grafo);
            }

            Validaciones.ReturnMenu();
        }

        // ---------- Historial de rotaciones ----------
        private void DemostrarRotaciones()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple("Historial de Rotaciones del Árbol AVL");

            if (tienda.Inventario is not ArbolAVL avl)
            {
                Vista.Aviso("El inventario no está representado por un árbol AVL.");
                Vista.Detalle("Cargue datos de demostración o asegúrese de que el inventario esté inicializado correctamente.");
                Validaciones.ReturnMenu();
                return;
            }

            if (avl.Cantidad == 0)
            {
                Vista.Aviso("El árbol está vacío: aún no se ha registrado ningún producto.");
                Vista.Detalle("No se han realizado modificaciones ni rotaciones todavía.");
                Validaciones.ReturnMenu();
                return;
            }

            if (avl.HistorialRotaciones.Count == 0)
            {
                Vista.Exito($"El árbol contiene {avl.Cantidad} producto(s), pero ninguna inserción, actualización o eliminación " +
                            "ha necesitado rotar el árbol hasta ahora.");
                Vista.Detalle("Esto puede ocurrir si los productos se insertaron en un orden que mantuvo el balance de forma natural.");
                Validaciones.ReturnMenu();
                return;
            }

            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.Title = new TableTitle("[bold]Historial de rotaciones[/]");
            tabla.AddColumn("N.º");
            tabla.AddColumn("Hora");
            tabla.AddColumn("Originada por");
            tabla.AddColumn("Tipo de rotación");
            tabla.AddColumn("Nodo pivote");
            tabla.AddColumn(new TableColumn("FB antes").RightAligned());

            foreach (var r in avl.HistorialRotaciones)
            {
                tabla.AddRow(
                    $"{r.Numero}",
                    Formato.Hora(r.Momento),
                    Markup.Escape(r.Motivo),
                    Markup.Escape(r.Tipo),
                    $"[bold]{Markup.Escape(r.NodoPivote)}[/]",
                    $"{r.FactorBalance:+0;-0;0}");
            }
            AnsiConsole.Write(tabla);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Total de rotaciones aplicadas:[/] {avl.HistorialRotaciones.Count} " +
                                   $"([cyan]{avl.RotacionesSimples}[/] simple(s) - [cyan]{avl.RotacionesDobles}[/] doble(s))");
            AnsiConsole.MarkupLine($"[grey]Estado actual del árbol: altura {avl.Altura}, raíz {Markup.Escape(avl.Raiz?.Producto.Codigo ?? "—")}.[/]");
            AnsiConsole.MarkupLine("[grey]LL/RR = rotación simple - LR/RL = rotación doble. \"FB antes\" es el factor de balance del nodo pivote justo antes de rotar.[/]");

            Validaciones.ReturnMenu();
        }

        // ---------- Mapa de la tienda ----------
        private void VerMapa()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple("Mapa de la Tienda - Grafo no dirigido y ponderado");
            Vista.Detalle("Vértices = secciones - Aristas = pasillos - Peso = metros. Se guarda como LISTA DE ADYACENCIA.");
            AnsiConsole.WriteLine();

            Vista.DibujarMapaAereo();
            AnsiConsole.WriteLine();

            var productosPorSeccion = tienda.ObtenerInventarioOrdenado()
                .GroupBy(p => tienda.Mapa.SeccionParaCategoria(p.Categoria).Etiqueta)
                .ToDictionary(g => g.Key, g => g.Count());

            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.AddColumn("Sección (vértice)");
            tabla.AddColumn(new TableColumn("Productos").RightAligned());
            tabla.AddColumn("Conectada con (vecino y metros)");

            foreach (var seccion in tienda.Mapa.Secciones)
            {
                productosPorSeccion.TryGetValue(seccion.Etiqueta, out int cantidad);
                string vecinos = string.Join(", ", seccion.Salientes.Select(a => $"{a.Destino.Etiqueta} ({Formato.Numero(a.Peso)} m)"));
                tabla.AddRow($"[bold]{Markup.Escape(seccion.Etiqueta)}[/]", cantidad.ToString(), Markup.Escape(vecinos));
            }

            AnsiConsole.Write(tabla);
            AnsiConsole.MarkupLine($"[bold]Vértices:[/] {tienda.Mapa.Grafo.CantidadVertices}   [bold]Aristas:[/] {tienda.Mapa.Grafo.CantidadAristas}");
            AnsiConsole.MarkupLine("[grey]Cada producto se ubica en la sección que coincide con su categoría (si no hay una igual, en \"Otros\").[/]");
            Validaciones.ReturnMenu();
        }

        // ---------- BFS y DFS ----------
        private void RecorridosBfsDfs()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Recorridos BFS y DFS del Mapa");

            var inicio = ElegirSeccion("Sección de partida (Enter = Entrada):", permitirVacio: true);
            var grafo = tienda.Mapa.Grafo;

            var bfs = grafo.RecorridoBFS(inicio);
            var dfs = grafo.RecorridoDFS(inicio);

            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Cyan1);
            tabla.AddColumn("[cyan]BFS - en anchura (usa una COLA)[/]");
            tabla.AddColumn("[magenta]DFS - en profundidad (usa RECURSIÓN)[/]");

            for (int i = 0; i < bfs.Count; i++)
            {
                tabla.AddRow(
                    $"{i + 1}. {Markup.Escape(bfs[i].Vertice.Etiqueta)} [grey](nivel {bfs[i].Nivel})[/]",
                    $"{i + 1}. {Markup.Escape(dfs[i].Etiqueta)}");
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(tabla);
            AnsiConsole.MarkupLine("[grey]BFS visita primero todo lo cercano: el nivel es el mínimo de pasillos a cruzar desde la partida.[/]");
            AnsiConsole.MarkupLine("[grey]DFS se interna por un camino hasta el fondo y retrocede solo cuando no puede seguir (backtracking).[/]");
            Validaciones.ReturnMenu();
        }

        // ---------- Dijkstra ----------
        private void RutaMasCorta()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Ruta más Corta (Dijkstra)");

            var origen = ElegirSeccion("Sección de origen (Enter = Entrada):", permitirVacio: true);
            AnsiConsole.WriteLine();

            string? codigo = Validaciones.LeerCodigoProducto(
                "Código del producto a localizar (Enter = elegir una sección):", permitirVacio: true);

            Vertice<string> destino;
            if (codigo != null)
            {
                Producto? producto = tienda.BuscarProducto(codigo);
                if (producto == null)
                {
                    Vista.Error($"No existe un producto con el código {codigo}.");
                    Validaciones.ReturnMenu();
                    return;
                }
                destino = tienda.Mapa.SeccionParaCategoria(producto.Categoria);
                Vista.Info($"{producto.Codigo} - {producto.Nombre} está en la sección \"{destino.Etiqueta}\".");
            }
            else
            {
                destino = ElegirSeccion("Sección de destino:", permitirVacio: false);
            }

            var traza = new List<string>();
            RutaEncontrada<string>? ruta = tienda.Mapa.Grafo.RutaMasCorta(origen, destino, traza);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]Pasos del algoritmo:[/]");
            foreach (string paso in traza)
                AnsiConsole.MarkupLine($"[grey]{Markup.Escape(paso)}[/]");

            AnsiConsole.WriteLine();
            if (ruta == null)
            {
                Vista.Error("No existe un camino entre esas secciones.");
            }
            else if (ruta.Camino.Count == 1)
            {
                Vista.Exito("Ya se encuentra en esa sección: distancia 0 metros.");
            }
            else
            {
                Vista.Exito($"Ruta más corta: {string.Join(" → ", ruta.Camino.Select(v => v.Etiqueta))}");
                AnsiConsole.MarkupLine($"[bold]Distancia total:[/] {Formato.Numero(ruta.Distancia)} metros - {ruta.Camino.Count - 1} pasillo(s)");
            }

            Validaciones.ReturnMenu();
        }

        private Vertice<string> ElegirSeccion(string mensaje, bool permitirVacio)
        {
            var secciones = tienda.Mapa.Secciones;

            var tabla = new Table().Border(TableBorder.Rounded).BorderColor(Color.Grey);
            tabla.AddColumn("N.º");
            tabla.AddColumn("Sección");
            for (int i = 0; i < secciones.Count; i++)
                tabla.AddRow($"{i + 1}", Markup.Escape(secciones[i].Etiqueta));
            AnsiConsole.Write(tabla);

            int? eleccion = Validaciones.LeerEnteroOpcional(mensaje, 1, secciones.Count, permitirVacio);
            return eleccion.HasValue ? secciones[eleccion.Value - 1] : tienda.Mapa.ObtenerSeccion(MapaTienda.Entrada);
        }
    }
}