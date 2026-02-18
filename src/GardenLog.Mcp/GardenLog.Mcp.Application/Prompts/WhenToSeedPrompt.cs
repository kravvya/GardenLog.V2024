using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace GardenLog.Mcp.Application.Prompts;

/// <summary>
/// MCP Prompt for analyzing when to seed a specific plant
/// </summary>
[McpServerPromptType]
public class WhenToSeedPrompt
{
    private const string PromptName = "when_to_seed";
    private static readonly Lazy<string> _cachedTemplate = new Lazy<string>(LoadTemplate);

    private static string LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "GardenLog.Mcp.Application.Resources.Prompts.WhenToSeedPrompt.txt";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [McpServerPrompt(Name = PromptName)]
    [Description("Analyzes when to seed a specific plant by comparing planned dates, historical data, and outcome quality. Returns detailed recommendation with confidence level.")]
    public Task<string> GetPromptAsync(
        [Description("Name of the plant to analyze (e.g., 'Tomatoes', 'Onions')")] string plantName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plantName))
        {
            throw new ArgumentException("plantName parameter is required", nameof(plantName));
        }

        return Task.FromResult(_cachedTemplate.Value.Replace("{plantName}", plantName));
    }
}
