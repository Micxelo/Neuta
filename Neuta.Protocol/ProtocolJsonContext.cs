using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neuta.Protocol;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    RespectRequiredConstructorParameters = true,
    RespectNullableAnnotations = true,
    AllowOutOfOrderMetadataProperties = true,
    Converters =
    [
        typeof(ProtocolEnumConverter<JobOperation>),
        typeof(ProtocolEnumConverter<JobPriority>),
        typeof(ProtocolEnumConverter<InferenceDevice>),
        typeof(ProtocolEnumConverter<BackendKind>),
        typeof(ProtocolEnumConverter<CancelStatus>),
        typeof(ProtocolEnumConverter<JobStatus>)
    ])]
[JsonSerializable(typeof(EditorRequest))]
[JsonSerializable(typeof(AgentResponse))]
[JsonSerializable(typeof(JobEvent))]
[JsonSerializable(typeof(InitializeResult))]
[JsonSerializable(typeof(ConfigureBackendResult))]
[JsonSerializable(typeof(SubmitResult))]
[JsonSerializable(typeof(CancelResult))]
[JsonSerializable(typeof(ShutdownResult))]
[JsonSerializable(typeof(PhraseIndexEntry[]))]
public partial class ProtocolJsonContext : JsonSerializerContext
{
}

internal sealed class ProtocolEnumConverter<T>() : JsonStringEnumConverter<T>(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    where T : struct, Enum;
