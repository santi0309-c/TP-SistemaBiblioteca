namespace Biblioteca.Models;

public class EstadoPrestamo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
}
