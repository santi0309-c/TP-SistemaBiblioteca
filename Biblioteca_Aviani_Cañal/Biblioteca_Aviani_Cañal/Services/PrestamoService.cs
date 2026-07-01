using Biblioteca.Data;
using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Services;

public class PrestamoService
{
    private readonly BibliotecaContext _ctx;

    public PrestamoService(BibliotecaContext ctx) => _ctx = ctx;

    // ─────────────────────────────────────────────────────────────────────────
    // PRÉSTAMO
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Registra un préstamo aplicando todas las RN.</summary>
    public (bool ok, string mensaje) RealizarPrestamo(int nroSocio, string isbn)
    {
        var socio = _ctx.Socios
            .Include(s => s.TipoSocio)
            .Include(s => s.Prestamos).ThenInclude(p => p.Estado)
            .Include(s => s.Prestamos).ThenInclude(p => p.Multa)
            .FirstOrDefault(s => s.NroSocio == nroSocio);

        if (socio == null) return (false, "Socio no encontrado.");

        // RN-01
        if (!socio.Activo) return (false, "El socio está inactivo y no puede realizar préstamos.");

        // RN-02
        bool tieneMultas = socio.Prestamos
            .Any(p => p.Multa != null && !p.Multa.Abonada);
        if (tieneMultas) return (false, "El socio tiene multas pendientes. Debe abonarlas antes de retirar libros.");

        // RN-04
        int prestamosActivos = socio.Prestamos.Count(p => p.EstadoId == 1); // 1 = Activo
        if (prestamosActivos >= socio.TipoSocio.MaxLibros)
            return (false, $"El socio alcanzó el límite de {socio.TipoSocio.MaxLibros} libros simultáneos.");

        var libro = _ctx.Libros
            .Include(l => l.Prestamos)
            .FirstOrDefault(l => l.ISBN == isbn);

        if (libro == null) return (false, "Libro no encontrado.");

        // RN-03: copias disponibles = total - préstamos activos de ese libro
        int copiasEnPrestamo = libro.Prestamos.Count(p => p.EstadoId == 1);
        int disponibles = libro.CantidadCopias - copiasEnPrestamo;
        if (disponibles <= 0) return (false, "NO_COPIAS"); // señal especial para ofrecer reserva

        // RN-05: fecha de vencimiento según tipo de socio
        var hoy = DateTime.Today;
        var vencimiento = hoy.AddDays(socio.TipoSocio.DiasPrestamo);

        var prestamo = new Prestamo
        {
            NroSocio = nroSocio,
            ISBN = isbn,
            FechaPrestamo = hoy,
            FechaVencimiento = vencimiento,
            EstadoId = 1, // Activo
            Renovado = false
        };

        _ctx.Prestamos.Add(prestamo);
        _ctx.SaveChanges();

        return (true, $"Préstamo registrado. Vence el {vencimiento:dd/MM/yyyy}.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DEVOLUCIÓN
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Registra la devolución de un préstamo activo o vencido.</summary>
    public (bool ok, string mensaje) RealizarDevolucion(int prestamoId)
    {
        var prestamo = _ctx.Prestamos
            .Include(p => p.Socio).ThenInclude(s => s.TipoSocio)
            .Include(p => p.Libro)
            .Include(p => p.Estado)
            .Include(p => p.Multa)
            .FirstOrDefault(p => p.Id == prestamoId);

        if (prestamo == null) return (false, "Préstamo no encontrado.");
        if (prestamo.FechaDevolucion != null) return (false, "El préstamo ya fue devuelto.");

        var hoy = DateTime.Today;
        prestamo.FechaDevolucion = hoy;
        prestamo.EstadoId = 2; // Devuelto

        string mensaje = "Devolución registrada exitosamente.";

        // RN-06: calcular multa si hay demora
        if (hoy > prestamo.FechaVencimiento)
        {
            int diasDemora = (hoy - prestamo.FechaVencimiento).Days;
            decimal multaPorDia = prestamo.Socio.TipoSocio.MultaPorDia;
            decimal monto = diasDemora * multaPorDia;

            var multa = new Multa
            {
                PrestamoId = prestamo.Id,
                Monto = monto,
                Abonada = false
            };
            _ctx.Multas.Add(multa);
            mensaje += $" Se generó una multa de ${monto:N2} por {diasDemora} día(s) de demora.";
        }

        _ctx.SaveChanges();

        // RN-07: notificar reserva pendiente si hay
        NotificarReservaPendiente(prestamo.ISBN);

        return (true, mensaje);
    }

    private void NotificarReservaPendiente(string isbn)
    {
        var reservaPendiente = _ctx.Reservas
            .Include(r => r.Socio)
            .Where(r => r.ISBN == isbn && r.EstadoId == 1) // Pendiente
            .OrderBy(r => r.FechaReserva)
            .FirstOrDefault();

        if (reservaPendiente == null) return;

        reservaPendiente.EstadoId = 2; // Cumplida
        _ctx.SaveChanges();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[NOTIFICACIÓN] El libro fue devuelto. Se notifica al socio: " +
                          $"{reservaPendiente.Socio.NombreCompleto} (Reserva #{reservaPendiente.Id}).");
        Console.ResetColor();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RESERVA
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Registra una reserva para un libro sin copias disponibles.</summary>
    public (bool ok, string mensaje) RealizarReserva(int nroSocio, string isbn)
    {
        var socio = _ctx.Socios.Find(nroSocio);
        if (socio == null) return (false, "Socio no encontrado.");

        // RN-01
        if (!socio.Activo) return (false, "El socio está inactivo y no puede realizar reservas.");

        // RN-08: no puede tener más de una reserva activa del mismo libro
        bool yaReservado = _ctx.Reservas
            .Any(r => r.NroSocio == nroSocio && r.ISBN == isbn && r.EstadoId == 1);
        if (yaReservado) return (false, "Ya tenés una reserva activa para este libro.");

        var reserva = new Reserva
        {
            NroSocio = nroSocio,
            ISBN = isbn,
            FechaReserva = DateTime.Today,
            EstadoId = 1 // Pendiente
        };

        _ctx.Reservas.Add(reserva);
        _ctx.SaveChanges();

        return (true, "Reserva registrada. Serás notificado cuando el libro esté disponible.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // RENOVACIÓN (consigna adicional)
    // ─────────────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) RenovarPrestamo(int prestamoId)
    {
        var prestamo = _ctx.Prestamos
            .Include(p => p.Socio).ThenInclude(s => s.TipoSocio)
            .Include(p => p.Libro).ThenInclude(l => l.Reservas)
            .FirstOrDefault(p => p.Id == prestamoId);

        if (prestamo == null) return (false, "Préstamo no encontrado.");
        if (prestamo.FechaDevolucion != null) return (false, "El préstamo ya fue devuelto.");
        if (prestamo.Renovado) return (false, "Este préstamo ya fue renovado anteriormente.");
        if (DateTime.Today > prestamo.FechaVencimiento) return (false, "No se puede renovar un préstamo vencido.");

        bool tieneReservas = prestamo.Libro.Reservas.Any(r => r.EstadoId == 1);
        if (tieneReservas) return (false, "No se puede renovar: el libro tiene reservas pendientes de otros socios.");

        int dias = prestamo.Socio.TipoSocio.DiasPrestamo;
        prestamo.FechaVencimiento = prestamo.FechaVencimiento.AddDays(dias);
        prestamo.Renovado = true;

        _ctx.SaveChanges();
        return (true, $"Préstamo renovado. Nueva fecha de vencimiento: {prestamo.FechaVencimiento:dd/MM/yyyy}.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PAGAR MULTA
    // ─────────────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) PagarMulta(int multaId)
    {
        var multa = _ctx.Multas.Find(multaId);
        if (multa == null) return (false, "Multa no encontrada.");
        if (multa.Abonada) return (false, "La multa ya estaba abonada.");

        multa.Abonada = true;
        _ctx.SaveChanges();
        return (true, $"Multa #{multaId} abonada por ${multa.Monto:N2}.");
    }
}
