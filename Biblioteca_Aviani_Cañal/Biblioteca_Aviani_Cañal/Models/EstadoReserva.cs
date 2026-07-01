namespace Biblioteca.Models;

public class EstadoReserva
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
