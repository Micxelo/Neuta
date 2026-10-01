using System.Text.Json;

namespace Neuta.Protocol;

public static class ProtocolIO
{
    public const string WarningType = "warning";

    /// <summary>
    /// Writes a ProtocolMessage to the given TextWriter.
    /// If no writer is provided, it defaults to <see cref="Console.Out"/> .
    /// </summary>
    public static void Write(ProtocolMessage message, TextWriter? writer = null)
    {
        writer ??= Console.Out;
        string json = JsonSerializer.Serialize(message, ProtocolContext.Default.ProtocolMessage);
        writer.Write(json);
        writer.Write('\n');
        writer.Flush();
    }

    /// <summary>
    /// Reads a ProtocolMessage from the given TextReader asynchronously.
    /// </summary>
    public static async Task<ProtocolMessage?> ReadAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        string? line = await reader.ReadLineAsync(cancellationToken);
        if (line is null or { Length: 0 })
            return null;
        return JsonSerializer.Deserialize(line, ProtocolContext.Default.ProtocolMessage);
    }
}
