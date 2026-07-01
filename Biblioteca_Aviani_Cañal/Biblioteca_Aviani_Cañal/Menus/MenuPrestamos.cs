using Biblioteca.Data;
using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Menus;

public class MenuPrestamos
{
    private readonly BibliotecaContext _ctx;
    private readonly PrestamoService _svc;

    public MenuPrestamos(BibliotecaContext ctx)
    {
        _ctx = ctx;
        _svc = new PrestamoService(ctx);
    }

    public void Mostrar()
    {
        bool salir = false;
        while (!salir)
        {
            ConsoleHelper.Titulo("PRÉSTAMOS");
            Console.WriteLine("  1. Registrar préstamo");
            Console.WriteLine("  2. Registrar devolución");
            Console.WriteLine("  3. Registrar reserva");
            Console.WriteLine("  4. Renovar préstamo");
            Console.WriteLine("  5. Pagar multa");
            Console.WriteLine("  6. Ver detalle de socio");
            Console.WriteLine("  0. Volver");
            Console.WriteLine();

            switch (ConsoleHelper.LeerEntero("Opción: "))
            {
                case 1: FlujoPrestamo(); break;
                case 2: FlujoDevolucion(); break;
                case 3: FlujoReserva(); break;
                case 4: FlujoRenovacion(); break;
                case 5: FlujoPagarMulta(); break;
                case 6: FlujoDetalleSocio(); break;
                case 0: salir = true; break;
                default: ConsoleHelper.Error("Opción inválida."); break;
            }
        }
    }

    // ─── Préstamo ─────────────────────────────────────────────────────────────
    private void FlujoPrestamo()
    {
        ConsoleHelper.Titulo("REGISTRAR PRÉSTAMO");

        int nroSocio = ConsoleHelper.LeerEntero("NroSocio: ");
        string busqueda = ConsoleHelper.LeerTexto("Título o autor del libro: ");

        var libros = _ctx.Libros
            .Where(l => EF.Functions.Like(l.Titulo, $"%{busqueda}%") ||
                        EF.Functions.Like(l.Autor, $"%{busqueda}%"))
            .ToList();

        if (!libros.Any()) { ConsoleHelper.Error("No se encontraron libros."); ConsoleHelper.Pausa(); return; }

        Console.WriteLine("\nResultados:");
        for (int i = 0; i < libros.Count; i++)
            Console.WriteLine($"  {i + 1}. [{libros[i].ISBN}] {libros[i].Titulo} — {libros[i].Autor}");

        int opcion = ConsoleHelper.LeerEntero("Elegí un número (0 para cancelar): ");
        if (opcion < 1 || opcion > libros.Count) { ConsoleHelper.Pausa(); return; }

        string isbn = libros[opcion - 1].ISBN;
        var (ok, msg) = _svc.RealizarPrestamo(nroSocio, isbn);

        if (ok) ConsoleHelper.Ok(msg);
        else if (msg == "NO_COPIAS")
        {
            ConsoleHelper.Info("No hay copias disponibles.");
            Console.Write("¿Querés hacer una reserva? (s/n): ");
            if (Console.ReadLine()?.Trim().ToLower() == "s")
            {
                var (okR, msgR) = _svc.RealizarReserva(nroSocio, isbn);
                if (okR) ConsoleHelper.Ok(msgR); else ConsoleHelper.Error(msgR);
            }
        }
        else ConsoleHelper.Error(msg);

        ConsoleHelper.Pausa();
    }

    // ─── Devolución ───────────────────────────────────────────────────────────
    private void FlujoDevolucion()
    {
        ConsoleHelper.Titulo("REGISTRAR DEVOLUCIÓN");

        int nroSocio = ConsoleHelper.LeerEntero("NroSocio: ");

        var prestamos = _ctx.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Estado)
            .Where(p => p.NroSocio == nroSocio && p.FechaDevolucion == null)
            .ToList();

        if (!prestamos.Any()) { ConsoleHelper.Error("No hay préstamos activos para ese socio."); ConsoleHelper.Pausa(); return; }

        Console.WriteLine("\nPréstamos activos:");
        foreach (var p in prestamos)
            Console.WriteLine($"  #{p.Id} — {p.Libro.Titulo} | Vence: {p.FechaVencimiento:dd/MM/yyyy} [{p.Estado.Nombre}]");

        int id = ConsoleHelper.LeerEntero("ID del préstamo a devolver (0 para cancelar): ");
        if (id == 0) { ConsoleHelper.Pausa(); return; }

        var (ok, msg) = _svc.RealizarDevolucion(id);
        if (ok) ConsoleHelper.Ok(msg); else ConsoleHelper.Error(msg);
        ConsoleHelper.Pausa();
    }

    // ─── Reserva ──────────────────────────────────────────────────────────────
    private void FlujoReserva()
    {
        ConsoleHelper.Titulo("REGISTRAR RESERVA");

        int nroSocio = ConsoleHelper.LeerEntero("NroSocio: ");
        string busqueda = ConsoleHelper.LeerTexto("Título o autor del libro: ");

        var libros = _ctx.Libros
            .Where(l => EF.Functions.Like(l.Titulo, $"%{busqueda}%") ||
                        EF.Functions.Like(l.Autor, $"%{busqueda}%"))
            .ToList();

        if (!libros.Any()) { ConsoleHelper.Error("No se encontraron libros."); ConsoleHelper.Pausa(); return; }

        for (int i = 0; i < libros.Count; i++)
            Console.WriteLine($"  {i + 1}. [{libros[i].ISBN}] {libros[i].Titulo} — {libros[i].Autor}");

        int opcion = ConsoleHelper.LeerEntero("Elegí un número: ");
        if (opcion < 1 || opcion > libros.Count) { ConsoleHelper.Pausa(); return; }

        var (ok, msg) = _svc.RealizarReserva(nroSocio, libros[opcion - 1].ISBN);
        if (ok) ConsoleHelper.Ok(msg); else ConsoleHelper.Error(msg);
        ConsoleHelper.Pausa();
    }

    // ─── Renovación ───────────────────────────────────────────────────────────
    private void FlujoRenovacion()
    {
        ConsoleHelper.Titulo("RENOVAR PRÉSTAMO");
        int id = ConsoleHelper.LeerEntero("ID del préstamo a renovar: ");
        var (ok, msg) = _svc.RenovarPrestamo(id);
        if (ok) ConsoleHelper.Ok(msg); else ConsoleHelper.Error(msg);
        ConsoleHelper.Pausa();
    }

    // ─── Pagar multa ──────────────────────────────────────────────────────────
    private void FlujoPagarMulta()
    {
        ConsoleHelper.Titulo("PAGAR MULTA");
        int nroSocio = ConsoleHelper.LeerEntero("NroSocio: ");

        var multas = _ctx.Multas
            .Include(m => m.Prestamo).ThenInclude(p => p.Libro)
            .Where(m => m.Prestamo.NroSocio == nroSocio && !m.Abonada)
            .ToList();

        if (!multas.Any()) { ConsoleHelper.Info("El socio no tiene multas pendientes."); ConsoleHelper.Pausa(); return; }

        Console.WriteLine("\nMultas pendientes:");
        foreach (var m in multas)
            Console.WriteLine($"  Multa #{m.Id} — Préstamo #{m.PrestamoId} ({m.Prestamo.Libro.Titulo}) — ${m.Monto:N2}");

        int id = ConsoleHelper.LeerEntero("ID de la multa a pagar (0 para cancelar): ");
        if (id == 0) { ConsoleHelper.Pausa(); return; }

        var (ok, msg) = _svc.PagarMulta(id);
        if (ok) ConsoleHelper.Ok(msg); else ConsoleHelper.Error(msg);
        ConsoleHelper.Pausa();
    }

    // ─── Detalle del socio ────────────────────────────────────────────────────
    private void FlujoDetalleSocio()
    {
        ConsoleHelper.Titulo("DETALLE DE SOCIO");
        var rptSvc = new ReporteService(_ctx);
        int nroSocio = ConsoleHelper.LeerEntero("NroSocio: ");

        var (socio, prestamos, reservas) = rptSvc.HistorialSocio(nroSocio);
        if (socio == null) { ConsoleHelper.Error("Socio no encontrado."); ConsoleHelper.Pausa(); return; }

        Console.WriteLine($"\n  Socio #{socio.NroSocio}: {socio.NombreCompleto}");
        Console.WriteLine($"  Tipo: {socio.TipoSocio.Nombre} | Activo: {(socio.Activo ? "Sí" : "No")}");
        ConsoleHelper.Separador();

        Console.WriteLine("\n  PRÉSTAMOS:");
        if (!prestamos.Any()) Console.WriteLine("    (ninguno)");
        foreach (var p in prestamos)
        {
            string multa = p.Multa != null ? $" | Multa: ${p.Multa.Monto:N2} ({(p.Multa.Abonada ? "abonada" : "pendiente")})" : "";
            Console.WriteLine($"    #{p.Id} — {p.Libro.Titulo} | {p.Estado.Nombre} | Vence: {p.FechaVencimiento:dd/MM/yyyy}{multa}");
        }

        Console.WriteLine("\n  RESERVAS:");
        if (!reservas.Any()) Console.WriteLine("    (ninguna)");
        foreach (var r in reservas)
            Console.WriteLine($"    #{r.Id} — {r.Libro.Titulo} | {r.Estado.Nombre} | Fecha: {r.FechaReserva:dd/MM/yyyy}");

        ConsoleHelper.Pausa();
    }
}
