namespace Neuta.Config;

public sealed record ConfigValidationIssue(
    string Field,
    string? Expected,
    string? Found,
    string Message);

/// <summary>
/// Exception thrown when a configuration key is semantically invalid
/// </summary>
public sealed class ConfigValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }
    public IReadOnlyList<ConfigValidationIssue> Issues { get; }

    public ConfigValidationException(IEnumerable<string> errors)
        : this(errors.Select(e => new ConfigValidationIssue(Field: string.Empty, Expected: null, Found: null, Message: e)).ToArray())
    {
    }

    public ConfigValidationException(IEnumerable<ConfigValidationIssue> issues) : this(issues.ToArray())
    {
    }

    private ConfigValidationException(ConfigValidationIssue[] issues)
        : base("Invalid configuration: " + string.Join("; ", issues.Select(i => i.Message)))
    {
        Issues = issues;
        Errors = issues.Select(i => i.Message).ToArray();
    }
}
