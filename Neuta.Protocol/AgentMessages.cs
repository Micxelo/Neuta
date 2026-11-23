using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Neuta.Protocol;

public abstract record AgentMessage;

/// <summary>
/// The Agent response to an editor request.
/// </summary>
public sealed record AgentResponse(
    string Id,
    JsonElement? Result = null,
    ProtocolError? Error = null) : AgentMessage, IJsonOnDeserialized, IJsonOnSerializing
{
    public static AgentResponse Success<T>(string id, T result, JsonTypeInfo<T> typeInfo) =>
        new(id, Result: JsonSerializer.SerializeToElement(result, typeInfo));

    public static AgentResponse Failure(string id, ProtocolError error) => new(id, Error: error);

    public T GetResult<T>(JsonTypeInfo<T> typeInfo)
    {
        if (Result is not { } result)
            throw new InvalidOperationException("The response has no result.");

        return result.Deserialize(typeInfo) ?? throw new JsonException("The response result cannot be null.");
    }

    void IJsonOnDeserialized.OnDeserialized() => Validate();
    void IJsonOnSerializing.OnSerializing() => Validate();

    private void Validate()
    {
        if (Error is null ? Result?.ValueKind != JsonValueKind.Object : Result is not null)
            throw new JsonException("A response must contain exactly one result object or error.");
    }
}

public sealed record InitializeResult(int ProtocolVersion);

public enum BackendKind
{
    WindowsTauClientServer,
    // Linux,
    // Not supported yet.
}

public sealed record BackendModel(string ModelId, string Fingerprint);

/// <summary>Option mappings use protocol option names as keys and backend CLI flags as values.</summary>
public sealed record ConfigureBackendResult(
    string BackendId,
    BackendKind Backend,
    string EngineFingerprint,
    IReadOnlyList<BackendModel> Models,
    IReadOnlyList<JobOperation> Operations,
    IReadOnlyDictionary<string, string> OptionMappings,
    int? Port = null,                                           // 12345, normally
    IReadOnlyDictionary<string, string>? ClientOptions = null,  // Used on Windows to pass options to the backend executable.
    IReadOnlyDictionary<string, string>? ExtraOptions = null);  // Other options listed in the backend's --help output, but not used by the protocol.

public sealed record SubmitResult(bool Accepted);

public enum CancelStatus
{
    CancellationRequested,
    AlreadyFinished
}

public sealed record CancelResult(CancelStatus Status);
public sealed record ShutdownResult;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "event")]
[JsonDerivedType(typeof(JobStartedEvent), "jobStarted")]
[JsonDerivedType(typeof(JobFinishedEvent), "jobFinished")]
public abstract record JobEvent(Guid JobId) : AgentMessage;

public sealed record JobStartedEvent(Guid JobId) : JobEvent(JobId);

public enum JobStatus
{
    Succeeded,
    Failed,
    Cancelled
}

public sealed record JobFinishedEvent(
    Guid JobId,
    JobStatus Status,
    JobResult? Result = null,
    ProtocolError? Error = null) : JobEvent(JobId), IJsonOnDeserialized, IJsonOnSerializing
{
    void IJsonOnDeserialized.OnDeserialized() => Validate();
    void IJsonOnSerializing.OnSerializing() => Validate();

    private void Validate()
    {
        bool valid = Status switch
        {
            JobStatus.Succeeded => Result is not null && Error is null,
            JobStatus.Failed => Result is null && Error is not null,
            JobStatus.Cancelled => Result is null && Error is null,
            _ => false
        };

        if (!valid)
            throw new JsonException("The job result or error does not match the terminal status.");
    }
}

public sealed record JobResult(
    IReadOnlyList<string> Files,
    string? File = null,
    long? StartFrameInTrack = null,
    long? FramePeriod100ns = null,
    int? FrameCount = null,
    long? StartSampleInTrack = null,
    int? SampleRate = null,
    int? Channels = null,
    int? BitDepth = null,
    long? SampleFrames = null) : IJsonOnDeserialized, IJsonOnSerializing
{
    void IJsonOnDeserialized.OnDeserialized() => ValidateFiles();
    void IJsonOnSerializing.OnSerializing() => ValidateFiles();

    private void ValidateFiles()
    {
        if (Files is null || Files.Count == 0 || Files.Any(static file => file is not (
            ArtifactNames.FullLabel or
            ArtifactNames.MonoLabel or
            ArtifactNames.TimingLabel or
            ArtifactNames.EnginePhraseList or
            ArtifactNames.PhraseIndex or
            ArtifactNames.F0 or
            ArtifactNames.MelSpec or
            ArtifactNames.Wav)))
        {
            throw new JsonException("A job result must list protocol artifact names.");
        }

        if (File is not null && !Files.Contains(File, StringComparer.Ordinal))
            throw new JsonException("The primary artifact must be listed in files.");
    }
}

public sealed record ProtocolError(
    string Code,
    string Message,
    string? Stage = null,
    int? ExitCode = null,
    int? NativeErrorCode = null,
    string? LogTail = null);

public static class ProtocolErrorCodes
{
    public const string ProtocolMismatch = "ProtocolMismatch";
    public const string BackendPortInUse = "BackendPortInUse";
    public const string BackendStartFailed = "BackendStartFailed";
    public const string EngineOptionNotApplied = "EngineOptionNotApplied";
    public const string PlatformNotSupported = "PlatformNotSupported";
}
