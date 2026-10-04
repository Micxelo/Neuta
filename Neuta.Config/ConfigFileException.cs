namespace Neuta.Config;

/// <summary>
/// Exception thrown when a configuration file is invalid or cannot be read.
/// </summary>
public sealed class ConfigFileException(string message, Exception innerException) : Exception(message, innerException)
{
}
