using Biblioteca.Data;
using Biblioteca.Services;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Menus;

public class MenuReportes
{
    private readonly BibliotecaContext _ctx;
    private readonly ReporteService _svc;

    public MenuReportes(BibliotecaContext ctx)
    {
        _ctx = ctx;
        _svc = new ReporteService(ctx);
    }

    public void Mostrar()
    {
        bool salir = false;
        while (!salir)
        {
            ConsoleHelper.Titulo("CONSULTAS Y REPORTES");
            Console.WriteLine("  1. Libros más prestados (Top 5)");
            Console.WriteLine("  2. Socios con multas pendientes");
            Console.WriteLine("  3. Préstamos vencidos");
            Console.WriteLine("  4. Disponibilidad de un libro");
            Console.WriteLine("  5. Historial de un socio");
            Console.WriteLine("  6. Ranking de socios más activos");
            Console.WriteLine("  0. Volver");
            Console.WriteLine();

            switch (ConsoleHelper.LeerEntero("Opción: "))
            {
                case 1: MostrarLibrosMasPrestados(); break;
                case 2: MostrarSociosConMultas(); break;
                case 3: MostrarPrestamosVencidos(); break;
                case 4: MostrarDisponibilidad(); break;
                case 5: MostrarHistorialSocio(); break;
                case 6: MostrarRankingSocios(); break;
                case 0: salir = true; break;
                default: ConsoleHelper.Error("Opción inválida."); break;
            }
        }
    }

    private void MostrarLibrosMasPrestados()
    {
        ConsoleHelper.Titulo("TOP 5 — LIBROS MÁS PRESTADOS");
        var lista = _svc.LibrosMasPrestados();
        if (!lista.Any()) { ConsoleHelper.Info("Sin datos."); ConsoleHelper.Pausa(); return; }

        int pos = 1;
        foreach (var (titulo, autor, cant) in lista)
            Console.WriteLine($"  {pos++}. {titulo} ({autor}) — {cant} préstamo(s)");

        ConsoleHelper.Pausa();
    }

    private void MostrarSociosConMultas()
    {
        ConsoleHelper.Titulo("SOCIOS CON MULTAS PENDIENTES");
        var lista = _svc.SociosConMultasPendientes();
        if (!lista.Any()) { ConsoleHelper.Ok("No hay socios con multas pendientes."); ConsoleHelper.Pausa(); return; }

        foreach (var (nro, nombre, total) in lista)
            Console.WriteLine($"  Socio #{nro} — {nombre} | Total adeudado: ${total:N2}");

        ConsoleHelper.Pausa();
    }

    private void MostrarPrestamosVencidos()
    {
        ConsoleHelper.Titulo("PRÉSTAMOS VENCIDOS");
        var lista = _svc.PrestamosVencidos();
        if (!lista.Any()) { ConsoleHelper.Ok("No hay préstamos vencidos."); ConsoleHelper.Pausa(); return; }

        foreach (var p in lista)
        {
            int diasDemora = (DateTime.Today - p.FechaVencimiento).Days;
            Console.WriteLine($"  #{p.Id} — {p.Libro.Titulo} | Socio: {p.Socio.NombreCompleto} " +
                              $"| Vencido hace {diasDemora} día(s) ({p.FechaVencimiento:dd/MM/yyyy})");
        }

        ConsoleHelper.Pausa();
    }

    private void MostrarDisponibilidad()
    {
        ConsoleHelper.Titulo("DISPONIBILIDAD DE LIBRO");
        string busqueda = ConsoleHelper.LeerTexto("ISBN o parte del título: ");

        var libro = _ctx.Libros
            .Where(l => l.ISBN == busqueda || EF.Functions.Like(l.Titulo, $"%{busqueda}%"))
            .FirstOrDefault();

        if (libro == null) { ConsoleHelper.Error("Libro no encontrado."); ConsoleHelper.Pausa(); return; }

        var (disponibles, reservas) = _svc.DisponibilidadLibro(libro.ISBN);

        Console.WriteLine($"\n  [{libro.ISBN}] {libro.Titulo} — {libro.Autor}");
        Console.WriteLine($"  Copias totales : {libro.CantidadCopias}");
        Console.WriteLine($"  Disponibles    : {disponibles}");
        Console.WriteLine($"  Reservas pend. : {reservas}");

        ConsoleHelper.Pausa();
    }

    private void MostrarHistorialSocio()
    {
        ConsoleHelper.Titulo("HISTORIAL DE SOCIO");
        int nroSocio = ConsoleHelper.LeerEntero("NroSocio: ");

        var (socio, prestamos, reservas) = _svc.HistorialSocio(nroSocio);
        if (socio == null) { ConsoleHelper.Error("Socio no encontrado."); ConsoleHelper.Pausa(); return; }

        Console.WriteLine($"\n  Socio #{socio.NroSocio}: {socio.NombreCompleto} ({socio.TipoSocio.Nombre})");
        ConsoleHelper.Separador();

        Console.WriteLine("\n  PRÉSTAMOS:");
        foreach (var p in prestamos)
        {
            string multa = p.Multa != null
                ? $" | Multa: ${p.Multa.Monto:N2} ({(p.Multa.Abonada ? "abonada" : "PENDIENTE")})"
                : "";
            Console.WriteLine($"    #{p.Id} {p.Libro.Titulo} [{p.Estado.Nombre}]" +
                              $" Vence:{p.FechaVencimiento:dd/MM/yy}{multa}");
        }

        Console.WriteLine("\n  RESERVAS:");
        foreach (var r in reservas)
            Console.WriteLine($"    #{r.Id} {r.Libro.Titulo} [{r.Estado.Nombre}] Fecha:{r.FechaReserva:dd/MM/yy}");

        ConsoleHelper.Pausa();
    }

    private void MostrarRankingSocios()
    {
        ConsoleHelper.Titulo("RANKING — TOP 10 SOCIOS MÁS ACTIVOS");
        var lista = _svc.RankingSocios();
        if (!lista.Any()) { ConsoleHelper.Info("Sin datos."); ConsoleHelper.Pausa(); return; }

        int pos = 1;
        foreach (var (socio, total, tieneMultas) in lista)
        {
            string alerta = tieneMultas ? " ⚠ multas pendientes" : "";
            Console.WriteLine($"  {pos++,2}. {socio.NombreCompleto} ({socio.TipoSocio.Nombre})" +
                              $" — {total} préstamo(s){alerta}");
        }

        ConsoleHelper.Pausa();
    }
}
