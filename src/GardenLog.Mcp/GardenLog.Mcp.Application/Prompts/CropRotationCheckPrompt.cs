using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace GardenLog.Mcp.Application.Prompts;

/// <summary>
/// MCP Prompt for validating crop rotation and suggesting best bed placement
/// </summary>
[McpServerPromptType]
public class CropRotationCheckPrompt
{
    private const string PromptName = "crop_rotation_check";
    private static readonly Lazy<string> _cachedTemplate = new Lazy<string>(LoadTemplate);

    private static string LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "GardenLog.Mcp.Application.Resources.Prompts.CropRotationCheckPrompt.txt";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [McpServerPrompt(Name = PromptName)]
    [Description("Validates crop rotation for a plant assigned to a bed in the current harvest cycle, checking for same-plant or same-family violations within 3 years.")]
    public Task<string> GetPromptAsync(
        [Description("Name of the plant to check (e.g., 'Tomatoes', 'Cabbage')")] string plantName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(plantName))
        {
            throw new ArgumentException("plantName parameter is required", nameof(plantName));
        }

        return Task.FromResult(_cachedTemplate.Value.Replace("{plantName}", plantName));
    }
}