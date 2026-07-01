namespace Biblioteca.Models;

public class Multa
{
    public int Id { get; set; }
    public int PrestamoId { get; set; }
    public decimal Monto { get; set; }
    public bool Abonada { get; set; }

    // Navegación
    public Prestamo Prestamo { get; set; } = null!;
}
