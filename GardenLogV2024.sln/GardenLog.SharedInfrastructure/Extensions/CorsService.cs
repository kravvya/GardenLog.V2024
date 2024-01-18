using Microsoft.AspNetCore.Cors.Infrastructure;

namespace GardenLog.SharedInfrastructure.Extensions
{
    public static class CorsOptionsExtensions
    {
        public static void AddGlWebPolicy(this CorsOptions options)
        {
            options.AddPolicy(name: "glWebPolicy",
                   policy =>
                   {
                       policy.WithOrigins("https://kravvya.github.io",
                           "https://localhost:7014",
                           "https://localhost:44318")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
                   });
        } 
    }
}
