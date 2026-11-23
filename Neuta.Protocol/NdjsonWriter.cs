using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Neuta.Protocol;

/// <summary>
/// Serializes and flushes whole NDJSON messages to a UTF-8 stream owned by the caller.
/// </summary>
public sealed class NdjsonWriter(Stream output)
{
    private readonly Stream output = output ?? throw new ArgumentNullException(nameof(output));
    private readonly SemaphoreSlim gate = new(1, 1);
    private bool writeFailed;

    public ValueTask WriteRequestAsync(EditorRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return WriteAsync(request, ProtocolJsonContext.Default.EditorRequest, cancellationToken);
    }

    public ValueTask WriteAgentMessageAsync(AgentMessage message, CancellationToken cancellationToken = default) =>
        message switch
        {
            AgentResponse response => WriteAsync(response, ProtocolJsonContext.Default.AgentResponse, cancellationToken),
            JobEvent jobEvent => WriteAsync(jobEvent, ProtocolJsonContext.Default.JobEvent, cancellationToken),
            null => throw new ArgumentNullException(nameof(message)),
            _ => throw new ArgumentException("Unknown Agent message type.", nameof(message))
        };

    private async ValueTask WriteAsync<T>(T message, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (writeFailed)
                throw new IOException("The NDJSON stream may contain an incomplete line after a write failure.");

            string json = JsonSerializer.Serialize(message, typeInfo);
            if (json.Length > ProtocolConstants.MaxLineCharacters)
                throw new JsonException($"An NDJSON line cannot exceed {ProtocolConstants.MaxLineCharacters} characters.");

            byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                // Once writing starts, finish the line even if the caller cancels.
                await output.WriteAsync(bytes.AsMemory()).ConfigureAwait(false);
                await output.FlushAsync().ConfigureAwait(false);
            }
            catch
            {
                writeFailed = true;
                throw;
            }
        }
        finally
        {
            gate.Release();
        }
    }
}
