using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace GardenLog.Mcp.Application.Resources;

/// <summary>
/// MCP Resource providing guidance for "When should I seed [plant]?" question pattern
/// </summary>
[McpServerResourceType]
public class WhenToSeedGuide
{
    private const string ResourceUri = "gardenlog://guides/when-to-seed";
    private static readonly Lazy<string> _cachedContent = new Lazy<string>(LoadContent);

    private static string LoadContent()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "GardenLog.Mcp.Application.Resources.Resources.WhenToSeedGuide.txt";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [McpServerResource(UriTemplate = ResourceUri)]
    [Description("Complete workflow guide for answering 'When should I seed [plant]?' questions using historical data and planned dates")]
    public Task<string> GetGuideAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cachedContent.Value);
    }
}
