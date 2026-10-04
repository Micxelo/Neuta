using System.Text.Json.Serialization;

namespace Neuta.Config;

public sealed class RendererConfig
{
    // config file version, must be 1
    public int Version { get; set; } = 1;

    public NeutrinoOptions Neutrino { get; set; } = new();
}

public sealed class NeutrinoOptions
{
    public string? InstallationDirectory { get; set; }

    public int SampleRate { get; set; } = 48000;
    public int BitDepth { get; set; } = 16;

    /// the number of parallel renderers to run, null for default (maximum)
    public int? Parallelism { get; set; }

    public GpuSelectionMode GpuMode { get; set; } = GpuSelectionMode.Disabled;
    // the index of the GPU to use when GpuMode is Specific, null for default (first GPU)
    public int? GpuId { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<GpuSelectionMode>))]
public enum GpuSelectionMode
{
    Disabled,
    Best,
    Specific
}
