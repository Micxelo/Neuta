using System.Text.Json.Serialization;

namespace Neuta.Protocol;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "method")]
[JsonDerivedType(typeof(InitializeRequest), "initialize")]
[JsonDerivedType(typeof(ConfigureBackendRequest), "configureBackend")]
[JsonDerivedType(typeof(SubmitRequest), "submit")]
[JsonDerivedType(typeof(CancelRequest), "cancel")]
[JsonDerivedType(typeof(ShutdownRequest), "shutdown")]
public abstract record EditorRequest(string Id);

public sealed record InitializeRequest(string Id, InitializeParameters Params) : EditorRequest(Id)
{
    public AgentResponse CreateResponse()
    {
        if (Params.ProtocolVersion != ProtocolConstants.Version)
        {
            return AgentResponse.Failure(Id, new ProtocolError(
                ProtocolErrorCodes.ProtocolMismatch,
                $"Protocol version {Params.ProtocolVersion} is unsupported. Expected version {ProtocolConstants.Version}."));
        }

        return AgentResponse.Success(Id, new InitializeResult(ProtocolConstants.Version), ProtocolJsonContext.Default.InitializeResult);
    }
}

public sealed record ConfigureBackendRequest(string Id, ConfigureBackendParameters Params) : EditorRequest(Id);
public sealed record SubmitRequest(string Id, SubmitParameters Params) : EditorRequest(Id);
public sealed record CancelRequest(string Id, CancelParameters Params) : EditorRequest(Id);
public sealed record ShutdownRequest(string Id) : EditorRequest(Id);

public sealed record InitializeParameters(int ProtocolVersion);

public sealed record ConfigureBackendParameters(
    string InstallationDirectory,
    int StartupTimeoutMs = ProtocolConstants.DefaultStartupTimeoutMs,
    int JobTimeoutMs = ProtocolConstants.DefaultJobTimeoutMs)
{
    public void Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(InstallationDirectory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(StartupTimeoutMs);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(JobTimeoutMs);
    }
}

/// <summary>
/// The priority of quened jobs. Preview jobs are processed first.
/// </summary>
public enum JobPriority
{
    Preview,
    Export,
    Background
}

public enum InferenceDevice
{
    Cpu,
    BestGpu,    // let NEUTRINO select GPU automatically
    Gpu         // select a specific GPU by GpuId
}

public sealed record InferenceOptions(
    int? Threads = null,
    int? Transpose = null,
    int? StyleShift = null,
    string? SupportModelId = null,
    InferenceDevice? Device = null,
    int? GpuId = null,
    int? SampleRate = null,
    int? BitDepth = null,
    bool? Verbose = null)
{
    internal void Validate(JobOperationContract operation)
    {
        if (Threads is <= 0)
            throw new ArgumentOutOfRangeException(nameof(Threads), "Thread count must be positive.");

        if (SupportModelId is not null)
            ArgumentException.ThrowIfNullOrWhiteSpace(SupportModelId);

        if (GpuId is < 0 || (Device == InferenceDevice.Gpu && GpuId is null))
            throw new ArgumentException("GPU selection requires a non-negative GPU ID.", nameof(GpuId));

        if (!operation.ProducesWav && (SampleRate.HasValue || BitDepth.HasValue))
            throw new ArgumentException("Sample rate and bit depth apply only to WAV operations.");

        if (SampleRate is <= 0)
            throw new ArgumentOutOfRangeException(nameof(SampleRate), "Sample rate must be positive.");

        if (BitDepth is not (null or 8 or 16 or 24 or 32))
            throw new ArgumentOutOfRangeException(nameof(BitDepth), "Bit depth must be 8, 16, 24 or 32.");
    }
}

public sealed record SubmitParameters(
    Guid JobId,
    string BackendId,
    JobOperation Operation,
    string? ModelId = null,
    int? EnginePhraseNumber = null,
    int? F0FrameCount = null,
    JobPriority? Priority = null,
    InferenceOptions? Options = null)
{
    /// <summary>
    /// Validates job parameters.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public void Validate()
    {
        if (JobId == Guid.Empty)
            throw new ArgumentException("A job ID must be a non-empty UUID.", nameof(JobId));

        ArgumentException.ThrowIfNullOrWhiteSpace(BackendId);

        var contract = JobOperationContract.For(Operation);

        if (contract.RequiresModel || ModelId is not null)
            ArgumentException.ThrowIfNullOrWhiteSpace(ModelId);

        if (EnginePhraseNumber is < 0 || (contract.RequiresPhraseNumber && EnginePhraseNumber is null))
            throw new ArgumentException("Phrase operations require a non-negative phrase number.", nameof(EnginePhraseNumber));

        if (F0FrameCount is <= 0 || (contract.RequiresF0FrameCount && F0FrameCount is null))
            throw new ArgumentException("Operations that read F0 require a positive frame count.", nameof(F0FrameCount));

        Options?.Validate(contract);
    }
}

public sealed record CancelParameters(Guid JobId);
