using Biblioteca.Data;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Data;

public static class DatabaseInitializer
{
    /// <summary>
    /// Crea las tablas (si no existen) y carga los datos del script SQL inicial.
    /// Si la DB ya tiene datos no hace nada, para no duplicar registros.
    /// </summary>
    public static void Initialize(BibliotecaContext context)
    {
        // Asegura que la base de datos existe (crea el archivo si no existe)
        context.Database.EnsureCreated();

        // Si ya hay datos de dominio, no volver a insertar
        if (context.TiposSocio.Any()) return;

        // Busca el script SQL relativo al ejecutable
        string[] candidatos = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "biblioteca.sql"),
            Path.Combine(Directory.GetCurrentDirectory(), "biblioteca.sql"),
            // En desarrollo, sube hasta encontrar el .sql
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "biblioteca.sql"),
        };

        string? sqlPath = candidatos.FirstOrDefault(File.Exists);

        if (sqlPath == null)
        {
            Console.WriteLine("[WARN] No se encontró biblioteca.sql. La DB queda vacía.");
            return;
        }

        string sql = File.ReadAllText(sqlPath);

        // Ejecuta el script completo. EF Core / SQLite acepta múltiples sentencias
        // separadas por punto y coma cuando se usa ExecuteSqlRaw con el script completo.
        // Dividimos manualmente para mayor compatibilidad.
        var sentencias = sql
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0 && !s.StartsWith("--") && !s.StartsWith("PRAGMA"));

        foreach (var sentencia in sentencias)
        {
            try
            {
                context.Database.ExecuteSqlRaw(sentencia);
            }
            catch
            {
                // Ignora errores de sentencias que ya existen (CREATE TABLE IF NOT EXISTS)
            }
        }

        Console.WriteLine("[OK] Base de datos inicializada con datos de ejemplo.");
    }
}
