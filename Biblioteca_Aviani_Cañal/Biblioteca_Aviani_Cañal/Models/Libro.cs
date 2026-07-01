namespace Biblioteca.Models;

public class Libro
{
    public string ISBN { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public int CantidadCopias { get; set; }

    // Navegación
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();

    /// <summary>Calcula las copias disponibles (total - activas prestadas).</summary>
    public int CopiasDisponibles(int prestamosActivos) => CantidadCopias - prestamosActivos;
}
