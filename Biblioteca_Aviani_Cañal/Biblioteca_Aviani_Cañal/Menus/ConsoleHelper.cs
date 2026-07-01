namespace Biblioteca.Menus;

public static class ConsoleHelper
{
    public static void Titulo(string texto)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"══════════════════════════════════════════");
        Console.WriteLine($"  {texto}");
        Console.WriteLine($"══════════════════════════════════════════");
        Console.ResetColor();
    }

    public static void Ok(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[✓] {msg}");
        Console.ResetColor();
    }

    public static void Error(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[✗] {msg}");
        Console.ResetColor();
    }

    public static void Info(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[i] {msg}");
        Console.ResetColor();
    }

    public static int LeerEntero(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int val)) return val;
            Error("Ingresá un número válido.");
        }
    }

    public static string LeerTexto(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? string.Empty;
    }

    public static void Separador() => Console.WriteLine(new string('─', 44));

    public static void Pausa()
    {
        Console.WriteLine("\nPresioná ENTER para continuar...");
        Console.ReadLine();
    }
}
