using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace GardenLog.Mcp.Application.Resources;

/// <summary>
/// MCP Resource providing guidance for analyzing all plants in a harvest cycle
/// </summary>
[McpServerResourceType]
public class FullCycleSeedPlanGuide
{
    private const string ResourceUri = "gardenlog://guides/full-cycle-seed-plan";
    private static readonly Lazy<string> _cachedContent = new Lazy<string>(LoadContent);

    private static string LoadContent()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "GardenLog.Mcp.Application.Resources.Resources.FullCycleSeedPlanGuide.txt";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [McpServerResource(UriTemplate = ResourceUri)]
    [Description("Complete workflow guide for analyzing seed timing for every plant in the current harvest cycle and comparing with planned dates")]
    public Task<string> GetGuideAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cachedContent.Value);
    }
}
