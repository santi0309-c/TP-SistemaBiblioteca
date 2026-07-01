namespace Biblioteca.Models;

public class Socio
{
    public int NroSocio { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TipoSocioId { get; set; }
    public bool Activo { get; set; }

    // Navegación
    public TipoSocio TipoSocio { get; set; } = null!;
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    public string NombreCompleto => $"{Nombre} {Apellido}";
}
