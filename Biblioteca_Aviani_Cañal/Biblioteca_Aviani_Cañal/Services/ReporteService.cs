using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Services;

public class ReporteService
{
    private readonly BibliotecaContext _ctx;

    public ReporteService(BibliotecaContext ctx) => _ctx = ctx;

    // ── 1. Libros más prestados ────────────────────────────────────────────────
    public List<(string Titulo, string Autor, int Cantidad)> LibrosMasPrestados(int top = 5)
    {
        return _ctx.Prestamos
            .Include(p => p.Libro)
            .GroupBy(p => new { p.ISBN, p.Libro.Titulo, p.Libro.Autor })
            .Select(g => new
            {
                g.Key.Titulo,
                g.Key.Autor,
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .Take(top)
            .AsEnumerable()
            .Select(x => (x.Titulo, x.Autor, x.Cantidad))
            .ToList();
    }

    // ── 2. Socios con multas pendientes ───────────────────────────────────────
    public List<(int NroSocio, string Nombre, decimal TotalMultas)> SociosConMultasPendientes()
    {
        return _ctx.Multas
            .Where(m => !m.Abonada)
            .Include(m => m.Prestamo).ThenInclude(p => p.Socio)
            .GroupBy(m => new { m.Prestamo.NroSocio, m.Prestamo.Socio.NombreCompleto })
            .Select(g => new
            {
                g.Key.NroSocio,
                g.Key.NombreCompleto,
                Total = g.Sum(m => m.Monto)
            })
            .AsEnumerable()
            .Select(x => (x.NroSocio, x.NombreCompleto, x.Total))
            .ToList();
    }

    // ── 3. Préstamos vencidos ─────────────────────────────────────────────────
    public List<Prestamo> PrestamosVencidos()
    {
        var hoy = DateTime.Today;
        return _ctx.Prestamos
            .Include(p => p.Socio)
            .Include(p => p.Libro)
            .Where(p => p.FechaDevolucion == null && p.FechaVencimiento < hoy)
            .OrderBy(p => p.FechaVencimiento)
            .ToList();
    }

    // ── 4. Disponibilidad de un libro ─────────────────────────────────────────
    public (int Disponibles, int ReservasPendientes) DisponibilidadLibro(string isbn)
    {
        var libro = _ctx.Libros
            .Include(l => l.Prestamos)
            .Include(l => l.Reservas)
            .FirstOrDefault(l => l.ISBN == isbn);

        if (libro == null) return (-1, -1);

        int prestadosActivos = libro.Prestamos.Count(p => p.EstadoId == 1);
        int disponibles = libro.CantidadCopias - prestadosActivos;
        int reservasPendientes = libro.Reservas.Count(r => r.EstadoId == 1);

        return (disponibles, reservasPendientes);
    }

    // ── 5. Historial de un socio ──────────────────────────────────────────────
    public (Socio? Socio, List<Prestamo> Prestamos, List<Reserva> Reservas) HistorialSocio(int nroSocio)
    {
        var socio = _ctx.Socios
            .Include(s => s.TipoSocio)
            .FirstOrDefault(s => s.NroSocio == nroSocio);

        if (socio == null) return (null, new(), new());

        var prestamos = _ctx.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Estado)
            .Include(p => p.Multa)
            .Where(p => p.NroSocio == nroSocio)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToList();

        var reservas = _ctx.Reservas
            .Include(r => r.Libro)
            .Include(r => r.Estado)
            .Where(r => r.NroSocio == nroSocio)
            .OrderByDescending(r => r.FechaReserva)
            .ToList();

        return (socio, prestamos, reservas);
    }

    // ── 6. Libros disponibles al arrancar ────────────────────────────────────
    public List<(Libro Libro, int Disponibles)> LibrosDisponibles()
    {
        return _ctx.Libros
            .Include(l => l.Prestamos)
            .AsEnumerable()
            .Select(l =>
            {
                int activos = l.Prestamos.Count(p => p.EstadoId == 1);
                return (l, l.CantidadCopias - activos);
            })
            .OrderBy(x => x.l.Titulo)
            .ToList();
    }

    // ── 7. Ranking de socios más activos (consigna adicional) ─────────────────
    public List<(Socio Socio, int TotalPrestamos, bool TieneMultas)> RankingSocios(int top = 10)
    {
        var prestamosXSocio = _ctx.Prestamos
            .GroupBy(p => p.NroSocio)
            .Select(g => new { NroSocio = g.Key, Total = g.Count() })
            .OrderByDescending(x => x.Total)
            .Take(top)
            .ToList();

        var resultado = new List<(Socio, int, bool)>();

        foreach (var item in prestamosXSocio)
        {
            var socio = _ctx.Socios
                .Include(s => s.TipoSocio)
                .Include(s => s.Prestamos).ThenInclude(p => p.Multa)
                .First(s => s.NroSocio == item.NroSocio);

            bool tieneMultas = socio.Prestamos.Any(p => p.Multa != null && !p.Multa.Abonada);
            resultado.Add((socio, item.Total, tieneMultas));
        }

        return resultado;
    }
}
