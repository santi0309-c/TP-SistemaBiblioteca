using Biblioteca.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public class BibliotecaContext : DbContext
{
    public DbSet<TipoSocio> TiposSocio { get; set; }
    public DbSet<EstadoPrestamo> EstadosPrestamo { get; set; }
    public DbSet<EstadoReserva> EstadosReserva { get; set; }
    public DbSet<Libro> Libros { get; set; }
    public DbSet<Socio> Socios { get; set; }
    public DbSet<Prestamo> Prestamos { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Multa> Multas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // La base de datos se crea/abre junto al ejecutable
        string dbPath = Path.Combine(AppContext.BaseDirectory, "biblioteca.db");
        options.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- Libro ----------
        modelBuilder.Entity<Libro>(e =>
        {
            e.HasKey(l => l.ISBN);
            e.Property(l => l.CantidadCopias).IsRequired();
        });

        // ---------- Socio ----------
        modelBuilder.Entity<Socio>(e =>
        {
            e.HasKey(s => s.NroSocio);
            e.HasOne(s => s.TipoSocio)
             .WithMany(t => t.Socios)
             .HasForeignKey(s => s.TipoSocioId);
        });

        // ---------- Prestamo ----------
        modelBuilder.Entity<Prestamo>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.Socio)
             .WithMany(s => s.Prestamos)
             .HasForeignKey(p => p.NroSocio);
            e.HasOne(p => p.Libro)
             .WithMany(l => l.Prestamos)
             .HasForeignKey(p => p.ISBN);
            e.HasOne(p => p.Estado)
             .WithMany(ep => ep.Prestamos)
             .HasForeignKey(p => p.EstadoId);
            e.HasOne(p => p.Multa)
             .WithOne(m => m.Prestamo)
             .HasForeignKey<Multa>(m => m.PrestamoId);
        });

        // ---------- Reserva ----------
        modelBuilder.Entity<Reserva>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasOne(r => r.Socio)
             .WithMany(s => s.Reservas)
             .HasForeignKey(r => r.NroSocio);
            e.HasOne(r => r.Libro)
             .WithMany(l => l.Reservas)
             .HasForeignKey(r => r.ISBN);
            e.HasOne(r => r.Estado)
             .WithMany(er => er.Reservas)
             .HasForeignKey(r => r.EstadoId);
        });

        // ---------- TipoSocio ----------
        modelBuilder.Entity<TipoSocio>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.MultaPorDia).HasColumnType("REAL");
        });

        // ---------- Multa ----------
        modelBuilder.Entity<Multa>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Monto).HasColumnType("REAL");
        });
    }
}
