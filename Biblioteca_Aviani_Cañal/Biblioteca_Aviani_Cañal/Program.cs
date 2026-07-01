using Biblioteca.Data;
using Biblioteca.Menus;
using Biblioteca.Services;
using Microsoft.EntityFrameworkCore;

using var ctx = new BibliotecaContext();
DatabaseInitializer.Initialize(ctx);

var rptSvc = new ReporteService(ctx);
var librosDisponibles = rptSvc.LibrosDisponibles();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("SISTEMA DE GESTIÓN DE BIBLIOTECA MUNICIPAL");
Console.ResetColor();
Console.WriteLine("\n Libros disponibles:\n");

foreach (var (libro, disp) in librosDisponibles)
    Console.WriteLine($"  [{libro.ISBN}] {libro.Titulo} — {libro.Autor}  ({disp}/{libro.CantidadCopias} copias)");

Console.WriteLine();

var menuPrestamos = new MenuPrestamos(ctx);
var menuReportes  = new MenuReportes(ctx);

bool salir = false;
while (!salir)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("MENÚ PRINCIPAL");
    Console.ResetColor();
    Console.WriteLine(" 1. Préstamos, devoluciones y reservas");
    Console.WriteLine(" 2. Consultas y reportes");
    Console.WriteLine(" 0. Salir");
    Console.WriteLine();
    Console.Write("Opción: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1": menuPrestamos.Mostrar(); break;
        case "2": menuReportes.Mostrar(); break;
        case "0": salir = true; break;
        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Opción inválida.");
            Console.ResetColor();
            break;
    }
}

Console.WriteLine("\nNos vemoss.");
