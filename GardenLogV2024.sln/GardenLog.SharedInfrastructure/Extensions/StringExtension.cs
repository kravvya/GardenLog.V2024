namespace GardenLog.SharedInfrastructure.Extensions;

public static class StringExtensions
{
    
    // Remove any spaces due to split and if `_` found change it to space.
    public static string ToCleanString(this string str)
        => str.Replace(" ", string.Empty)
              .Replace('_', ' ');
}
