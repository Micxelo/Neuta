namespace Neuta.Protocol;

public sealed class ProtocolMessage
{
    public string Type { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public List<Issue>? Issues { get; set; }    // only used for exceptions
}

public sealed record Issue(string Field, string? Expected, string? Found, string Message);
