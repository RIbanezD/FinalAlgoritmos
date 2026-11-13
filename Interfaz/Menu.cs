using DonJuanMarket_Ruben_Ibañez.Estructuras;
using DonJuanMarket_Ruben_Ibañez.Modelos;
using DonJuanMarket_Ruben_Ibañez.Servicios;
using DonJuanMarket_Ruben_Ibañez.Utilidades;
using Spectre.Console;

namespace DonJuanMarket_Ruben_Ibañez.Interfaz
{
    public class Menu
    {
        private readonly Supermercado tienda;
        private readonly MenuGrafos menuGrafos;

        public Menu(Supermercado tienda)
        {
            this.tienda = tienda ?? throw new ArgumentNullException(nameof(tienda));
            menuGrafos = new MenuGrafos(tienda);
        }

        // ---------- Ciclo principal ----------
        public void Ejecutar()
        {
            try
            {
                Bienvenida();
                CargaInicial();
                BuclePrincipal();
            }
            catch (EntradaAgotadaException){}

            Despedida();
        }

        private void BuclePrincipal()
        {
            bool salir = false;
            while (!salir)
            {
                try
                {
                    Vista.Encabezado(tienda);
                    MostrarMenuPrincipal();

                    string opcion = Validaciones.LeerTexto("Seleccione una opción:", permitirVacio: true, permitirVolver: false);
                    salir = ProcesarOpcion(opcion);
                }
                catch (OperacionCanceladaException)
                {
                    AnsiConsole.WriteLine();
                    Vista.Aviso("Operación cancelada. Regresando al menú principal...");
                    Validaciones.ReturnMenu();
                }
                catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or IOException or UnauthorizedAccessException)
                {
                    AnsiConsole.WriteLine();
                    Vista.Error("Ocurrió un problema inesperado y la operación no se completó.");
                    Vista.Detalle(ex.Message);
                    Validaciones.ReturnMenu();
                }
            }
        }

        private bool ProcesarOpcion(string opcion)
        {
            switch (opcion)
            {
                case "1": RegistrarProducto(); break;
                case "2": MenuBuscarProducto(); break;
                case "3": ActualizarProducto(); break;
                case "4": EliminarProducto(); break;
                case "5": MostrarInventario(); break;
                case "6": AgregarCliente(); break;
                case "7": AtenderCliente(); break;
                case "8": MostrarClientesEnEspera(); break;
                case "9": MostrarEstadisticas(); break;
                case "10": return ConfirmarSalida();
                case "11": menuGrafos.Ejecutar(); break;
                case "12": MenuDatos(); break;
                default:
                    Vista.Error("Opción inválida. Elija un número del 1 al 12.");
                    Validaciones.ReturnMenu();
                    break;
            }
            return false;
        }

        // ---------- Pantallas generales ----------

        private static void Bienvenida()
        {
            Vista.Limpiar();
            AnsiConsole.Write(new FigletText("Don Juan").Centered().Color(Color.DeepSkyBlue1));
            AnsiConsole.Write(new FigletText("Market").Centered().Color(Color.DeepSkyBlue1));
            AnsiConsole.Write(new Rule("[bold white]Sistema de Gestión de Inventario y Atención de Clientes[/]").Centered());
            AnsiConsole.MarkupLine("[grey]Procesos y Algoritmos II - Árbol AVL - Cola de Prioridad (Heap) - Grafos[/]");
            AnsiConsole.WriteLine();
            Validaciones.ReturnMenu();
        }

        // c arga de datos de prueba
        private void CargaInicial()
        {
            try
            {
                Vista.Encabezado(tienda);

                if (GestorArchivos.ExistenDatos())
                {
                    Vista.Aviso("Se detectaron datos guardados anteriormente (inventario.csv / clientes.csv).");
                    if (Validaciones.Confirmar("¿Desea importarlos antes de continuar?"))
                    {
                        EjecutarImportacion();
                        Validaciones.ReturnMenu();
                    }
                }
                else
                {
                    Vista.Detalle("No hay datos guardados: el sistema inicia vacío.");
                    if (Validaciones.Confirmar("¿Desea cargar datos de demostración para probar el sistema?"))
                    {
                        CargarDemostracion();
                        Validaciones.ReturnMenu();
                    }
                }
            }
            catch (OperacionCanceladaException){}
        }

        private void MostrarMenuPrincipal()
        {

            var panel = Vista.PanelMenu(
                $"[bold yellow]INVENTARIO ({tienda.Inventario.Cantidad})[/]\n" +
                "[green][[1]][/] Registrar producto\n" +
                "[green][[2]][/] Buscar producto (código, nombre o categoría)\n" +
                "[green][[3]][/] Actualizar producto\n" +
                "[green][[4]][/] Eliminar producto\n" +
                "[green][[5]][/] Mostrar inventario ordenado\n" +
                "\n" +
                $"[bold yellow]ATENCIÓN DE CLIENTES ({tienda.ColaClientes.Cantidad})[/]\n" +
                "[green][[6]][/] Agregar cliente a la cola\n" +
                "[green][[7]][/] Atender siguiente cliente\n" +
                "[green][[8]][/] Mostrar clientes en espera\n" +
                "\n" +
                "[bold yellow]REPORTES[/]\n" +
                "[green][[9]][/] Mostrar estadísticas del sistema\n" +
                "\n" +
                "[bold yellow]SISTEMA[/]\n" +
                "[green][[10]][/] Salir\n" +
                "\n" +
                "[bold magenta]EXTRA / Sandbox [/]\n" +
                "[green][[11]][/] Sandbox\n" +
                "[green][[12]][/] Guardar / importar datos (CSV)",
                "Menú Principal");
            AnsiConsole.Write(panel);

            AnsiConsole.MarkupLine(
                $"[grey]Productos:[/] [bold cyan]{tienda.Inventario.Cantidad}[/]   [grey]|[/]   " +
                $"[grey]En espera:[/] [bold cyan]{tienda.ColaClientes.Cantidad}[/]   [grey]|[/]   " +
                $"[grey]Atendidos:[/] [bold green]{tienda.ObtenerAtendidos().Count}[/]");
            AnsiConsole.WriteLine();
        }

        private bool ConfirmarSalida()
        {
            if (tienda.HayCambiosSinGuardar &&
                Validaciones.Confirmar("Hay cambios sin guardar. ¿Desea guardarlos antes de salir?"))
            {
                GuardarDatos();
            }
            return true;
        }

        private static void Despedida()
        {
            Vista.Limpiar();
            AnsiConsole.Write(new FigletText("Hasta pronto").Centered().Color(Color.DeepSkyBlue1));
            AnsiConsole.MarkupLine("[grey]Gracias por usar Don Juan Market.[/]");
            AnsiConsole.WriteLine();
        }

        // ---------- INVENTARIO ----------
        private void RegistrarProducto()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Registrar Producto");

            string codigo = SolicitarCodigoNuevo();
            AnsiConsole.MarkupLine($"[grey]Código asignado:[/] [bold]{Markup.Escape(codigo)}[/]");
            AnsiConsole.WriteLine();

            string nombre = Validaciones.LeerTexto("Nombre del producto:");
            AnsiConsole.WriteLine();
            string categoria = SolicitarCategoria();
            AnsiConsole.WriteLine();
            double precio = Validaciones.LeerDecimal("Precio en quetzales (mayor que 0):", 0, 1_000_000);
            int stock = Validaciones.LeerEntero("Cantidad en stock (0 o más):", 0, 1_000_000);

            var producto = new Producto(codigo, nombre, categoria, precio, stock);
            bool insertado = tienda.RegistrarProducto(producto);

            AnsiConsole.WriteLine();
            if (insertado)
            {
                Vista.Exito($"Producto {producto.Codigo} insertado correctamente.");
                Vista.TarjetaProducto(producto, "Producto registrado");
            }
            else
            {
                Vista.Error($"Ya existe un producto con el código {producto.Codigo}.");
            }

            Validaciones.ReturnMenu();
        }

        // codigo producto
        private string SolicitarCodigoNuevo()
        {
            string sugerido = tienda.GenerarSiguienteCodigo();

            AnsiConsole.Write(Vista.PanelMenu(
                $"[green][[1]][/] Correlativo automático (siguiente: [bold]{Markup.Escape(sugerido)}[/])\n" +
                "[green][[2]][/] Código manual (ejemplo: PROD025)",
                "Código del Producto"));
            AnsiConsole.WriteLine();

            while (true)
            {
                string opcion = Validaciones.LeerTexto("¿Cómo desea asignar el código? (1-2):");
                switch (opcion)
                {
                    case "1": return sugerido;
                    case "2": return LeerCodigoManual();
                    default: Vista.Error("Opción inválida. Elija 1 o 2."); break;
                }
            }
        }

        private string LeerCodigoManual()
        {
            while (true)
            {
                string codigo = Validaciones.LeerCodigoProducto("Código del producto:")!;

                if (tienda.BuscarProducto(codigo) != null)
                {
                    Vista.Error($"Ya existe un producto con el código {codigo}.");
                    continue;
                }
                return codigo;
            }
        }

        // menu de busqueda
        private void MenuBuscarProducto()
        {
            bool volver = false;
            while (!volver)
            {
                Vista.Encabezado(tienda);
                AnsiConsole.Write(Vista.PanelMenu(
                    "[green][[1]][/] Buscar por código (coincidencia exacta)\n" +
                    "[green][[2]][/] Buscar por nombre (coincidencia parcial)\n" +
                    "[green][[3]][/] Buscar por categoría\n" +
                    "[green][[4]][/] Volver al menú principal",
                    "Buscar Producto"));
                AnsiConsole.WriteLine();

                switch (Validaciones.LeerTexto("Seleccione una opción:", permitirVacio: true, permitirVolver: false))
                {
                    case "1": BuscarPorCodigo(); break;
                    case "2": BuscarPorNombre(); break;
                    case "3": BuscarPorCategoria(); break;
                    case "4": volver = true; break;
                    default:
                        Vista.Error("Opción inválida. Elija un número del 1 al 4.");
                        Validaciones.ReturnMenu();
                        break;
                }
            }
        }

        private void BuscarPorCodigo()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Buscar Producto por Código");

            string codigo = Validaciones.LeerCodigoProducto("Código del producto a buscar:")!;

            Producto? producto = tienda.BuscarProducto(codigo);

            AnsiConsole.WriteLine();
            if (producto == null)
            {
                Vista.Aviso("Producto no encontrado.");
            }
            else
            {
                Vista.Exito("Producto encontrado:");
                Vista.TarjetaProducto(producto, "Resultado de la búsqueda");
            }

            Validaciones.ReturnMenu();
        }

        private void BuscarPorNombre()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Buscar Producto por Nombre");

            string texto = Validaciones.LeerTexto("Nombre o parte del nombre a buscar:");
            List<Producto> resultados = tienda.BuscarPorNombre(texto);

            AnsiConsole.WriteLine();
            if (resultados.Count == 0)
                Vista.Aviso($"No se encontraron productos cuyo nombre contenga \"{texto}\".");
            else
                Vista.TablaInventario(resultados, $"Resultados para \"{texto}\" ({resultados.Count})");

            Validaciones.ReturnMenu();
        }

        private void BuscarPorCategoria()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Buscar Producto por Categoría");

            Vista.MostrarTablaCategorias();
            AnsiConsole.WriteLine();
            string entrada = Validaciones.LeerTexto("Elija un número de la lista o escriba una categoría a buscar:");
            string categoria = Vista.ResolverCategoria(entrada);

            List<Producto> resultados = tienda.BuscarPorCategoria(categoria);

            AnsiConsole.WriteLine();
            if (resultados.Count == 0)
                Vista.Aviso($"No se encontraron productos en la categoría \"{categoria}\".");
            else
                Vista.TablaInventario(resultados, $"Categoría: {categoria} ({resultados.Count})");

            Validaciones.ReturnMenu();
        }

        // categorias de producto
        private string SolicitarCategoria(string? actual = null)
        {
            Vista.MostrarTablaCategorias();
            string mensaje = actual == null
                ? "Elija el número de una categoría de la lista, o escriba el nombre de una nueva:"
                : $"Categoría (número de la lista, texto nuevo, o Enter para conservar \"{actual}\"):";

            while (true)
            {
                string entrada = Validaciones.LeerTexto(mensaje, permitirVacio: actual != null);

                if (entrada.Length == 0)
                {
                    if (actual != null) return string.Empty; // conservar la categoría actual
                    Vista.Error("Debe elegir un número de la lista o escribir una categoría.");
                    continue;
                }

                return Vista.ResolverCategoria(entrada);
            }
        }

        private void ActualizarProducto()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Actualizar Producto");

            string codigo = Validaciones.LeerCodigoProducto("Código del producto a actualizar:")!;
            Producto? actual = tienda.BuscarProducto(codigo);

            if (actual == null)
            {
                Vista.Error($"No existe un producto con el código {codigo}.");
                Validaciones.ReturnMenu();
                return;
            }

            AnsiConsole.WriteLine();
            Vista.TarjetaProducto(actual, "Datos actuales");
            Vista.Detalle("Pulse Enter para conservar el valor actual.");
            AnsiConsole.WriteLine();

            string nombre = Validaciones.LeerTexto($"Nuevo nombre (actual: {Markup.Escape(actual.Nombre)}):", permitirVacio: true);
            AnsiConsole.WriteLine();
            Vista.Detalle("Categoría: elija un número de la lista, escriba una nueva, o pulse Enter para conservar la actual.");
            string categoria = SolicitarCategoria(actual.Categoria);
            AnsiConsole.WriteLine();
            double? precio = Validaciones.LeerDecimalOpcional($"Nuevo precio (actual: {Formato.Moneda(actual.Precio)}):", 0, 1_000_000);
            int? stock = Validaciones.LeerEnteroOpcional($"Nueva cantidad en stock (actual: {actual.Stock}):", 0, 1_000_000);

            AnsiConsole.WriteLine();
            if (nombre.Length == 0 && categoria.Length == 0 && precio == null && stock == null)
            {
                Vista.Aviso("No se indicó ningún cambio. El producto permanece igual.");
                Validaciones.ReturnMenu();
                return;
            }

            bool actualizado = tienda.ActualizarProducto(
                codigo,
                nombre.Length > 0 ? nombre : actual.Nombre,
                categoria.Length > 0 ? categoria : actual.Categoria,
                precio ?? actual.Precio,
                stock ?? actual.Stock);

            if (actualizado)
            {
                Vista.Exito("Producto actualizado correctamente.");
                Vista.TarjetaProducto(tienda.BuscarProducto(codigo)!, "Datos actualizados");
            }
            else
            {
                Vista.Error($"No existe un producto con el código {codigo}.");
            }

            Validaciones.ReturnMenu();
        }

        private void EliminarProducto()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Eliminar Producto");

            string codigo = Validaciones.LeerCodigoProducto("Código del producto a eliminar:")!;
            Producto? producto = tienda.BuscarProducto(codigo);

            if (producto == null)
            {
                Vista.Error($"No existe un producto con el código {codigo}.");
                Validaciones.ReturnMenu();
                return;
            }

            AnsiConsole.WriteLine();
            Vista.TarjetaProducto(producto, "Producto a eliminar");
            AnsiConsole.WriteLine();

            if (!Validaciones.Confirmar($"¿Confirma que desea eliminar el producto {codigo}?"))
            {
                Vista.Detalle("Operación cancelada. El producto permanece en el inventario.");
                Validaciones.ReturnMenu();
                return;
            }

            AnsiConsole.WriteLine();
            if (tienda.EliminarProducto(codigo))
            {
                Vista.Exito($"Producto {codigo} eliminado correctamente.");
            }
            else
            {
                Vista.Error($"No existe un producto con el código {codigo}.");
            }

            Validaciones.ReturnMenu();
        }

        private void MostrarInventario()
        {
            Vista.Encabezado(tienda);

            List<Producto> productos = tienda.ObtenerInventarioOrdenado();
            if (productos.Count == 0)
            {
                Vista.Aviso("El inventario está vacío. Registre productos con la opción 1.");
            }
            else
            {
                Vista.TablaInventario(productos);

                if (Vista.DetalleTecnicoActivo)
                {
                    AnsiConsole.WriteLine();
                    Vista.Detalle(tienda.Inventario is ArbolAVL avl
                        ? $"- Recorrido in-orden recursivo del Árbol AVL (altura {avl.Altura})."
                        : "");
                }
            }

            Validaciones.ReturnMenu();
        }

        // ---------- ATENCIÓN DE CLIENTES ----------
        private void AgregarCliente()
        {
            Vista.Encabezado(tienda);
            Vista.Titulo("Agregar Cliente a la Cola");

            string nombre = Validaciones.LeerTexto("Nombre del cliente:");
            AnsiConsole.WriteLine();
            Vista.LeyendaTipos();
            TipoCliente tipo = Validaciones.LeerTipoCliente("Tipo de cliente (1-3):");
            int articulos = Validaciones.LeerEntero("Número de productos que va a comprar:", 1, 1000);

            var cliente = new Cliente(nombre, tipo, articulos);
            int posicion = tienda.AgregarCliente(cliente);

            AnsiConsole.WriteLine();
            Vista.Exito($"Cliente {cliente.Nombre} agregado correctamente.");
            Vista.Info($"Posición estimada en la cola: {posicion}");

            Validaciones.ReturnMenu();
        }

        private void AtenderCliente()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple("Atender Siguiente Cliente");

            Cliente? cliente = tienda.AtenderSiguiente();
            if (cliente == null)
            {
                Vista.Aviso("No hay clientes en espera. La cola de prioridad está vacía.");
            }
            else
            {
                AnsiConsole.MarkupLine("[bold]Atendiendo cliente:[/]");
                Vista.TarjetaCliente(cliente, "Cliente en caja");
                AnsiConsole.WriteLine();
                Vista.Exito("Cliente atendido correctamente.");
                Vista.Detalle($"Clientes restantes en la cola: {tienda.ColaClientes.Cantidad}");
            }

            Validaciones.ReturnMenu();
        }

        private void MostrarClientesEnEspera()
        {
            Vista.Encabezado(tienda);

            List<Cliente> clientes = tienda.ObtenerClientesEnEspera();
            if (clientes.Count == 0)
                Vista.Aviso("No hay clientes en espera.");
            else
                Vista.TablaClientesEnEspera(clientes);

            Validaciones.ReturnMenu();
        }

        // ---------- REPORTES ----------
        private void MostrarEstadisticas()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple("Estadísticas del Sistema");

            EstadisticasSistema e = tienda.ObtenerEstadisticas();

            // Inventario
            var inventario = NuevaTablaDatos();
            AgregarFila(inventario, "Estructura", e.EstructuraInventario);
            AgregarFila(inventario, "Productos registrados", e.TotalProductos.ToString());
            AgregarFila(inventario, "Unidades en stock", e.UnidadesEnStock.ToString());
            AgregarFila(inventario, "Valor del inventario", Formato.Moneda(e.ValorInventario));
            AgregarFila(inventario, "Productos agotados", e.ProductosAgotados == 0 ? "0" : $"[red]{e.ProductosAgotados}[/]");
            AgregarFila(inventario, $"Con stock bajo (≤ {EstiloVisual.UmbralStockBajo})", e.ProductosStockBajo == 0 ? "0" : $"[yellow]{e.ProductosStockBajo}[/]");
            if (e.MasCaro != null)
                AgregarFila(inventario, "Producto más caro", $"{e.MasCaro.Codigo} - {e.MasCaro.Nombre} ({Formato.Moneda(e.MasCaro.Precio)})");
            if (e.MayorStock != null)
                AgregarFila(inventario, "Mayor existencia", $"{e.MayorStock.Codigo} - {e.MayorStock.Nombre} ({e.MayorStock.Stock} u.)");
            if (e.AlturaArbol.HasValue)
            {
                AgregarFila(inventario, "Altura del árbol AVL", e.AlturaArbol.Value.ToString());
                AgregarFila(inventario, "Raíz actual del árbol", e.RaizArbol ?? "—");
                AgregarFila(inventario, "Rotaciones aplicadas", $"{e.RotacionesSimples} simples - {e.RotacionesDobles} dobles");
            }
            AnsiConsole.Write(PanelDatos(inventario, "Inventario"));

            if (e.Categorias.Count > 0)
            {
                Color[] paleta = { Color.Cyan1, Color.Gold1, Color.Green, Color.Orange1, Color.Magenta1, Color.DeepSkyBlue1 };
                var grafico = new BarChart().Width(60).Label("[bold]Valor del inventario por categoría (Q)[/]").CenterLabel();
                for (int i = 0; i < e.Categorias.Count; i++)
                    grafico.AddItem(Markup.Escape(e.Categorias[i].Categoria), Math.Round(e.Categorias[i].Valor, 2), paleta[i % paleta.Length]);
                AnsiConsole.WriteLine();
                AnsiConsole.Write(grafico);
            }

            // atencion al cliente
            AnsiConsole.WriteLine();
            var clientes = NuevaTablaDatos();
            AgregarFila(clientes, "Estructura", e.EstructuraCola);
            AgregarFila(clientes, "Clientes en espera", e.ClientesEnEspera.ToString());
            AgregarFila(clientes, "   VIP", e.EsperaVip.ToString());
            AgregarFila(clientes, "   Adulto mayor", e.EsperaAdultoMayor.ToString());
            AgregarFila(clientes, "   Regular", e.EsperaRegular.ToString());
            AgregarFila(clientes, "Artículos por procesar", e.ArticulosPorProcesar.ToString());
            AgregarFila(clientes, "Siguiente en atender", e.SiguienteCliente == null ? "—" : $"{e.SiguienteCliente.Nombre} ({e.SiguienteCliente.Tipo.ATexto()})");
            AgregarFila(clientes, "Clientes atendidos", e.ClientesAtendidos.ToString());
            AgregarFila(clientes, "   VIP", e.AtendidosVip.ToString());
            AgregarFila(clientes, "   Adulto mayor", e.AtendidosAdultoMayor.ToString());
            AgregarFila(clientes, "   Regular", e.AtendidosRegular.ToString());
            AgregarFila(clientes, "Artículos procesados", e.ArticulosProcesados.ToString());
            AgregarFila(clientes, "Espera promedio", e.EsperaPromedio.HasValue ? Formato.Duracion(e.EsperaPromedio.Value) : "—");
            AnsiConsole.Write(PanelDatos(clientes, "Atención de clientes"));

            Validaciones.ReturnMenu();
        }

        private static Grid NuevaTablaDatos()
        {
            var grid = new Grid();
            grid.AddColumn(new GridColumn().NoWrap().PadRight(3));
            grid.AddColumn();
            return grid;
        }

        private static void AgregarFila(Grid grid, string etiqueta, string valor) =>
            grid.AddRow($"[bold]{Markup.Escape(etiqueta)}[/]", valor.StartsWith('[') ? valor : Markup.Escape(valor));

        private static Panel PanelDatos(Grid contenido, string titulo) => new(contenido)
        {
            Header = new PanelHeader($" {titulo} "),
            Border = BoxBorder.Rounded,
            BorderStyle = Style.Parse("cyan"),
            Padding = new Padding(2, 1),
            Expand = false
        };

        // ---------- GUARDAR / IMPORTAR ----------
        private void MenuDatos()
        {
            bool volver = false;
            while (!volver)
            {
                Vista.Encabezado(tienda);
                AnsiConsole.Write(Vista.PanelMenu(
                    "[green][[1]][/] Guardar datos en archivos CSV\n" +
                    "[green][[2]][/] Importar datos desde archivos CSV\n" +
                    "[green][[3]][/] Cargar datos de demostración\n" +
                    "[green][[4]][/] Volver al menú principal",
                    "Guardar / Importar Datos"));
                AnsiConsole.WriteLine();

                switch (Validaciones.LeerTexto("Seleccione una opción:", permitirVacio: true, permitirVolver: false))
                {
                    case "1": GuardarDatos(); break;
                    case "2": ImportarDatos(); break;
                    case "3": CargarDemostracionConPantalla(); break;
                    case "4": volver = true; break;
                    default:
                        Vista.Error("Opción inválida. Elija un número del 1 al 4.");
                        Validaciones.ReturnMenu();
                        break;
                }
            }
        }

        private void GuardarDatos()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple("Guardar Datos");

            var enEspera = tienda.ObtenerClientesEnEspera();
            var todosLosClientes = enEspera.Concat(tienda.ObtenerAtendidos()).ToList();
            var productos = tienda.ObtenerInventarioOrdenado();

            if (productos.Count == 0 && todosLosClientes.Count == 0)
            {
                Vista.Aviso("No hay datos para guardar.");
                Validaciones.ReturnMenu();
                return;
            }

            try
            {
                GestorArchivos.Guardar(productos, todosLosClientes);
                tienda.MarcarComoGuardado();

                Vista.Exito($"Se guardaron {productos.Count} producto(s) y {todosLosClientes.Count} cliente(s) " +
                            $"({enEspera.Count} en espera y {tienda.ObtenerAtendidos().Count} atendidos).");
                Vista.Detalle($"Archivos: {GestorArchivos.RutaInventario}");
                Vista.Detalle($"          {GestorArchivos.RutaClientes}");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Vista.Error("No se pudieron guardar los archivos. Verifique que no estén abiertos en otro programa.");
                Vista.Detalle(ex.Message);
            }

            Validaciones.ReturnMenu();
        }

        private void ImportarDatos()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple("Importar Datos");

            if (!GestorArchivos.ExistenDatos())
            {
                Vista.Error("No se encontraron los archivos inventario.csv ni clientes.csv en la carpeta del programa.");
                Vista.Detalle($"Carpeta: {AppContext.BaseDirectory}");
                Validaciones.ReturnMenu();
                return;
            }

            EjecutarImportacion();
            Validaciones.ReturnMenu();
        }

        private void EjecutarImportacion()
        {
            ResultadoImportacion datos;
            try
            {
                datos = GestorArchivos.Importar();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Vista.Error("No se pudo leer el archivo. Verifique que no esté abierto en otro programa.");
                Vista.Detalle(ex.Message);
                return;
            }

            bool yaHayClientes = tienda.ColaClientes.Cantidad > 0 || tienda.ObtenerAtendidos().Count > 0;
            if (yaHayClientes && datos.Clientes.Count > 0 &&
                !Validaciones.Confirmar("Ya hay clientes registrados. ¿Importar también los clientes del archivo?"))
            {
                datos.Clientes.Clear();
            }

            var r = tienda.Importar(datos);

            Vista.Exito($"Importación finalizada: {r.ProductosNuevos} producto(s) nuevo(s), {r.ClientesEnCola} cliente(s) en cola " +
                        $"y {r.ClientesAtendidos} ya atendido(s).");
            if (r.ProductosDuplicados > 0)
                Vista.Aviso($"{r.ProductosDuplicados} producto(s) se omitieron porque su código ya existía.");

            if (datos.Errores.Count > 0)
            {
                Vista.Aviso($"{datos.Errores.Count} línea(s) no se pudieron procesar:");
                foreach (string error in datos.Errores.Take(10))
                    Vista.Detalle($"  - {error}");
            }
        }

        private void CargarDemostracionConPantalla()
        {
            Vista.Encabezado(tienda);
            Vista.TituloSimple("Datos de Demostración");
            CargarDemostracion();
            Validaciones.ReturnMenu();
        }

        private void CargarDemostracion()
        {
            var (productos, clientes) = tienda.CargarDemostracion();
            Vista.Exito($"Se cargaron {productos} producto(s) y {clientes} cliente(s) de ejemplo.");
            Vista.Detalle("Los productos se insertaron en desorden a propósito: revise el AVL en el Sandbox (opción 11).");
        }
    }
}