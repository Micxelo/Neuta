using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neuta.Config;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(RendererConfig))]
public partial class ConfigJsonContext : JsonSerializerContext
{
}
