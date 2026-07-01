namespace Biblioteca.Models;

public class Reserva
{
    public int Id { get; set; }
    public int NroSocio { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public DateTime FechaReserva { get; set; }
    public int EstadoId { get; set; }

    // Navegación
    public Socio Socio { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
    public EstadoReserva Estado { get; set; } = null!;
}
