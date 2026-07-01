namespace Biblioteca.Models;

public class Prestamo
{
    public int Id { get; set; }
    public int NroSocio { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaDevolucion { get; set; }
    public int EstadoId { get; set; }
    public bool Renovado { get; set; }

    // Navegación
    public Socio Socio { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
    public EstadoPrestamo Estado { get; set; } = null!;
    public Multa? Multa { get; set; }

    public bool EstaVencido => FechaDevolucion == null && DateTime.Today > FechaVencimiento;
}
