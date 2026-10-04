namespace Neuta.Config;

public static class ConfigValidator
{
    public static void Validate(RendererConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var issues = new List<ConfigValidationIssue>();
        if (config.Version != 1)
            issues.Add(new ConfigValidationIssue(
                Field: "render.version",
                Expected: "1",
                Found: config.Version.ToString(),
                Message: $"render.version must be 1, found {config.Version}."));

        if (config.Neutrino is null)
        {
            issues.Add(new ConfigValidationIssue(
                Field: "render.neutrino",
                Expected: "an object",
                Found: "null",
                Message: "render.neutrino must be defined."));
        }
        else
        {
            var n = config.Neutrino;
            if (n.SampleRate <= 0)
                issues.Add(new ConfigValidationIssue(
                    Field: "render.sampleRate",
                    Expected: "a positive integer",
                    Found: n.SampleRate.ToString(),
                    Message: $"render.sampleRate must be positive, found {n.SampleRate}."));
            if (n.BitDepth is not 8 and not 16 and not 24 and not 32)
                issues.Add(new ConfigValidationIssue(
                    Field: "render.bitDepth",
                    Expected: "8, 16, 24, or 32",
                    Found: n.BitDepth.ToString(),
                    Message: $"render.bitDepth must be one of 8, 16, 24, or 32, found {n.BitDepth}."));
            if (n.Parallelism is <= 0)
                issues.Add(new ConfigValidationIssue(
                    Field: "render.parallelism",
                    Expected: "null or a positive integer",
                    Found: n.Parallelism?.ToString() ?? "null",
                    Message: $"render.parallelism must be null or positive, found {n.Parallelism}."));
            if (!Enum.IsDefined(n.GpuMode))
                issues.Add(new ConfigValidationIssue(
                    Field: "render.gpuMode",
                    Expected: string.Join(", ", Enum.GetNames<GpuSelectionMode>()),
                    Found: n.GpuMode.ToString(),
                    Message: $"render.gpuMode is unsupported, found {n.GpuMode}."));
            if (n.GpuMode == GpuSelectionMode.Specific && n.GpuId is null or < 0)
                issues.Add(new ConfigValidationIssue(
                    Field: "render.gpuIndex",
                    Expected: "a non-negative integer",
                    Found: n.GpuId?.ToString() ?? "null",
                    Message: $"render.gpuIndex must be non-negative when render.gpuMode is specific, found {n.GpuId}."));
        }

        if (issues.Count > 0)
            throw new ConfigValidationException(issues);
    }
}
