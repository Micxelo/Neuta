using System.Buffers;
using System.Text;
using System.Text.Json;

namespace Neuta.Protocol;

/// <summary>
/// Reads protocol messages from a UTF-8 stream owned by the caller.
/// </summary>
public sealed class NdjsonReader(Stream input)
{
    // A UTF-16 code unit takes at most three UTF-8 bytes.
    private const int MaxLineBytes = ProtocolConstants.MaxLineCharacters * 3 + 1;
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private static readonly JsonDocumentOptions DocumentOptions = new() { AllowDuplicateProperties = false };

    private readonly Stream input = input ?? throw new ArgumentNullException(nameof(input));
    private readonly byte[] buffer = new byte[4096];
    private readonly ArrayBufferWriter<byte> line = new();
    private int start;
    private int end;
    private bool oversized;

    /// <summary>
    /// Reads an Editor request, or returns null at the end of the stream.
    /// </summary>
    public ValueTask<EditorRequest?> ReadRequestAsync(CancellationToken cancellationToken = default) =>
        ReadAsync(static message =>
        {
            if (!message.TryGetProperty("method", out _))
                throw new JsonException("An Editor request must contain a method.");

            return message.Deserialize(ProtocolJsonContext.Default.EditorRequest)!;
        }, cancellationToken);

    /// <summary>
    /// Reads an Agent response or event, or returns null at the end of the stream.
    /// </summary>
    public ValueTask<AgentMessage?> ReadAgentMessageAsync(CancellationToken cancellationToken = default) =>
        ReadAsync<AgentMessage>(static message =>
        {
            if (!message.TryGetProperty("event", out _))
                return message.Deserialize(ProtocolJsonContext.Default.AgentResponse)!;

            if (message.TryGetProperty("id", out _))
                throw new JsonException("An Agent event uses jobId instead of a request ID.");

            return message.Deserialize(ProtocolJsonContext.Default.JobEvent)!;
        }, cancellationToken);

    private async ValueTask<T?> ReadAsync<T>(Func<JsonElement, T> deserialize, CancellationToken cancellationToken)
        where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (await ReadLineAsync(cancellationToken).ConfigureAwait(false) is not { } bytes)
            return null;

        try
        {
            ValidateLine(bytes.Span);
            using var document = JsonDocument.Parse(bytes, DocumentOptions);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new JsonException("Each NDJSON line must contain a JSON object.");

            return deserialize(document.RootElement);
        }
        finally
        {
            line.Clear();
        }
    }

    private async ValueTask<ReadOnlyMemory<byte>?> ReadLineAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (start == end)
            {
                end = await input.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
                start = 0;
                if (end == 0)
                {
                    if (line.WrittenCount == 0 && !oversized)
                        return null;

                    line.Clear();
                    oversized = false;
                    throw new JsonException("The NDJSON stream ended before the last line delimiter.");
                }
            }

            ReadOnlySpan<byte> unread = buffer.AsSpan(start, end - start);
            int newline = unread.IndexOf((byte)'\n');
            int length = newline < 0 ? unread.Length : newline;

            if (!oversized)
            {
                oversized = length > MaxLineBytes - line.WrittenCount;
                if (!oversized)
                    line.Write(unread[..length]);
            }

            start += length;
            if (newline < 0)
                continue;

            start++;
            if (oversized)
            {
                line.Clear();
                oversized = false;
                throw LineTooLong();
            }

            ReadOnlyMemory<byte> bytes = line.WrittenMemory;
            return bytes.Span is [.., (byte)'\r'] ? bytes[..^1] : bytes;
        }
    }

    private static void ValidateLine(ReadOnlySpan<byte> line)
    {
        if (line.Contains((byte)'\r'))
            throw new JsonException("An NDJSON line cannot contain an unescaped carriage return.");

        try
        {
            if (Utf8.GetCharCount(line) > ProtocolConstants.MaxLineCharacters)
                throw LineTooLong();
        }
        catch (DecoderFallbackException exception)
        {
            throw new JsonException("The NDJSON line is not valid UTF-8.", exception);
        }
    }

    private static JsonException LineTooLong() =>
        new($"An NDJSON line cannot exceed {ProtocolConstants.MaxLineCharacters} characters.");
}
