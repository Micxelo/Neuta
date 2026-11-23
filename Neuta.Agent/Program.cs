using System.Reflection;
using System.Text;

namespace Neuta.Agent;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
        Console.OutputEncoding = utf8;

        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        AssemblyName assemblyName = assembly.GetName();

        if (args.Any(a => a is "--help" or "-h"))
        {
            var assemblyVersion = assemblyName.Version?.ToString() ?? "unknown";
            string productVersion =
                assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assemblyVersion;

            Console.WriteLine($"Neuta.Agent version {productVersion}");
            Console.WriteLine("Usage: Neuta.Agent --session-root <absolute-directory>");
            return 0;
        }

        if (args is not ["--session-root", var root] || !Path.IsPathFullyQualified(root))
        {
            Console.Error.WriteLine("Invalid arguments. Expected --session-root followed by an absolute directory.");
            return 2;
        }

        try
        {
            using var input = new StreamReader(Console.OpenStandardInput(), utf8, detectEncodingFromByteOrderMarks: false);
            using var output = new StreamWriter(Console.OpenStandardOutput(), utf8) { NewLine = "\n", AutoFlush = true };

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }
}
