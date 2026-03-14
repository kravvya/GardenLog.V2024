using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace GardenLog.Mcp.Application.Resources;

/// <summary>
/// MCP Resource providing guidance for crop rotation validation and warnings
/// </summary>
[McpServerResourceType]
public class CropRotationGuide
{
    private const string ResourceUri = "gardenlog://guides/crop-rotation";
    private static readonly Lazy<string> _cachedContent = new Lazy<string>(LoadContent);

    private static string LoadContent()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "GardenLog.Mcp.Application.Resources.Resources.CropRotationGuide.txt";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    [McpServerResource(UriTemplate = ResourceUri)]
    [Description("Complete workflow guide for validating crop rotation and warning about planting same families in same beds")]
    public Task<string> GetGuideAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cachedContent.Value);
    }
}