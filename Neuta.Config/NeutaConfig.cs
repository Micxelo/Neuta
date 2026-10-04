namespace Neuta.Config;

public static class NeutaConfig
{
    public static ConfigStore<RendererConfig> LoadRendererConfig(string configDirectory) =>
        new(
            ConfigPaths.Renderer(configDirectory),
            ConfigJsonContext.Default.RendererConfig,
            static () => new RendererConfig(),
            ConfigValidator.Validate);
}
