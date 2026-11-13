
  DON JUAN MARKET - Sistema de Gestion de Inventario y Atencion de Clientes
================================================================================

DESCRIPCION
-----------
Aplicacion de consola en C# (.NET) para la tienda "Don Juan Market" que
implementa, desde cero (sin librerias externas de estructuras de datos):

  * Arbol AVL (arbol binario de busqueda auto-balanceado) para el inventario
    de productos, con rotaciones simples (LL, RR) y dobles (LR, RL).
  * Cola de Prioridad (Min Heap) para la atencion de clientes, con prioridad
    VIP > ADULTO_MAYOR > REGULAR y desempate FIFO.

DIAGRAMA
-------------
[![Architecture diagram of ribanezd/finalalgoritmos](https://gitdiagram.com/ribanezd/finalalgoritmos/diagram.png)](https://gitdiagram.com/ribanezd/finalalgoritmos?utm_source=readme&utm_medium=picture)

ESTRUCTURA DEL PROYECTO
------------------------
  DonJuanMarket_Ruben_Ibañez/
  |-- DonJuanMarket_Ruben_Ibañez.csproj   Archivo de proyecto (.NET 8)
  |-- Program.cs                          Punto de entrada
  |-- Modelos/                            Producto, Cliente, TipoCliente
  |-- Estructuras/                        ArbolAVL, ColaPrioridad,
  |                                       GrafoInventario, GrafoAtencion,
  |                                       NodoAVL, Grafo, Contratos
  |-- Servicios/                          Supermercado, MapaTienda,
  |                                       GestorArchivos, EstadisticasSistema
  |-- Interfaz/                           Menu, MenuGrafos, Vista
  |-- Utilidades/                         Validaciones, Formato, EstiloVisual
  |-- Diagramas UML/                      Diagramas de clases
  |-- Documentacion/                      Manual de usuario

