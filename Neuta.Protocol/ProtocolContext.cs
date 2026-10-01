using System.Text.Json.Serialization;

namespace Neuta.Protocol;

// NDJSON source generator context for the protocol messages
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(ProtocolMessage))]
public partial class ProtocolContext : JsonSerializerContext
{
}
