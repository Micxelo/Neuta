namespace Neuta.Config;

public static class ConfigPaths
{
    public const string ConfigDirectoryName = "configs";
    public const string RendererConfigFileName = "renderer.json";

    /// <summary>
    /// Returns the path to the configuration directory for the program.
    /// </summary>
    public static string ForProgramDirectory(string? programDirectory = null) =>
        Path.GetFullPath(Path.Combine(programDirectory ?? AppContext.BaseDirectory, ConfigDirectoryName));

    /// <summary>
    /// Returns the path to the renderer configuration file.
    /// </summary>
    public static string Renderer(string configDirectory) =>
        Path.Combine(Path.GetFullPath(configDirectory), RendererConfigFileName);
}
