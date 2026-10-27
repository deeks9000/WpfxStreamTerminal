using System.Text;

namespace SerialStreamShell;

public static class CommandShell
{
    private static readonly Dictionary<string, Func<string>> _commands = new(StringComparer.OrdinalIgnoreCase);

    public static void Initialize()
    {
        AddCommand("help", Cmd_Help);
        AddCommand("hello", () => "Hello from serial server");
        AddCommand("name", () => "SAL 9000");
        AddCommand("version", () => "CommandShell 1.0.1");
        AddCommand("time", Cmd_Time);
    }

    public static string GetResponse(string command)
    {
        if (_commands.TryGetValue(command, out var handler))
            return handler();

        if (string.IsNullOrEmpty(command))
            return "Empty line received. " + Cmd_Help();

        return $"ERROR: Command '{command}' not found";
    }
     
    private static void AddCommand(string key, Func<string> handler)
    {
        _commands[key] = handler;
    }

    private static string Cmd_Help()
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("Available shell commands:");

        foreach (var cmd in _commands.Keys)
        {
            sb.AppendLine($"  {cmd}");
        }
        
        return sb.ToString().TrimEnd();
    }

    private static string Cmd_Time()
    {
        return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
    }   
}
