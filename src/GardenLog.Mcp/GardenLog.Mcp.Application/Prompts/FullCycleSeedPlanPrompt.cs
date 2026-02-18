using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace GardenLog.Mcp.Application.Prompts;

/// <summary>
/// MCP Prompt for analyzing seed timing for all plants in current harvest cycle
/// </summary>
[McpServerPromptType]
public class FullCycleSeedPlanPrompt
{
    private const string PromptName = "full_cycle_seed_plan";
    private static readonly Lazy<string> _cachedContent = new Lazy<string>(LoadContent);

    private static string LoadContent()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "GardenLog.Mcp.Application.Resources.Prompts.FullCycleSeedPlanPrompt.txt";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [McpServerPrompt(Name = PromptName)]
    [Description("Analyzes seed timing for every plant in the current harvest cycle, compares with planned dates, and flags discrepancies that need attention.")]
    public Task<string> GetPromptAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cachedContent.Value);
    }
}
