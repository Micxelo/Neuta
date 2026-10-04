using Neuta.Config;
using Neuta.Protocol;

namespace Neuta.Agent;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        ConfigStore<RendererConfig>? _rendererConfigStore;
        RendererConfig? _rendererConfig;

        Console.Error.WriteLine($"Neuta executable path: {Environment.ProcessPath}");

        var configDir = ConfigPaths.ForProgramDirectory();
        var configFile = ConfigPaths.Renderer(configDir);
        if (!File.Exists(configFile))
            Console.Error.WriteLine(
                $"Renderer config file not found at {configFile}, creating default config. NEUTRINO will not work.");
            
        try
        {
            _rendererConfigStore = NeutaConfig.LoadRendererConfig(configDir);
            _rendererConfig = await _rendererConfigStore.ReadAsync();
        }
        catch (ConfigFileException ex)
        {
            Console.Error.WriteLine($"Failed to load renderer config file: {ex.Message}.");
            return 1;
        }
        catch (ConfigValidationException ex)
        {
            // format-valid but semantically invalid config file
            Console.Error.WriteLine($"Renderer config file has warnings: {ex.Message}");
            ProtocolIO.Write(new ProtocolMessage
            {
                Type = ProtocolIO.WarningType,
                Message = "renderer.json contains invalid values",
                Issues = ex.Issues.Select(i => new Issue(i.Field, i.Expected, i.Found, i.Message)).ToList(),
            });
        }
        
        return 0;
    }
}
