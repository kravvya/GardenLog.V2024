using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace GardenLog.SharedInfrastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static StringContent ToJsonStringContent<T>(this T @object, JsonSerializerOptions? options = default)
        {
            return new StringContent(
                JsonSerializer.Serialize(@object, options),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);
        }
    }
}
