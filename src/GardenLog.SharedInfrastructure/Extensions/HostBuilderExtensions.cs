using Azure.Communication.Email;
using GardenLog.SharedInfrastructure.ApiClients;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Formatting.Elasticsearch;
using System.Security.Claims;

namespace GardenLog.SharedInfrastructure.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseLogging(this IHostBuilder builder) =>
          builder.UseSerilog((context, logger) =>
          {
              logger.Enrich.FromLogContext();
              logger.Enrich.WithSpan();
              logger.Enrich.WithClientIp();
              logger.Enrich.WithRequestHeader("User-Agent");

              logger.ReadFrom.Configuration(context.Configuration);

              if (context.HostingEnvironment.IsDevelopment())
              {
                  logger.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} {TraceId} {Level: u3} {Message}{NewLine}{Exception}");
              }
              else
              {
                  logger.WriteTo.Console(new ElasticsearchJsonFormatter());
              }
          });

        public static void RegisterPoly(this WebApplicationBuilder builder, string registryName, int numberOfRetries)
        {
            var registry = new Polly.Registry.PolicyRegistry()
            {
                {
                    registryName,
                     HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(numberOfRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        onRetry: (outcome, timespan, retryAttempt, context) =>
                        {
                            context.GetLogger()?.LogWarning("Failed to post event to target consumer. Delaying for {delay}ms, then making retry {retry}. Error message {exception}", timespan.TotalMilliseconds, retryAttempt, outcome.Exception);
                        })
                },
            };

            builder.Services.AddPolicyRegistry(registry);
        }

        public static void RegisterHttpClient<TInterfaceType, TConcreteType>(this WebApplicationBuilder builder)
            where TConcreteType : class, TInterfaceType
            where TInterfaceType : class
        {
            builder.Services.AddHttpClient<IAuth0AuthenticationApiClient, Auth0AuthenticationApiClient>();
            builder.Services.AddHttpClient<TInterfaceType, TConcreteType>().AddHttpMessageHandler(provider =>
                new Auth0AuthenticationHandler(provider.GetRequiredService<IAuth0AuthenticationApiClient>(), provider.GetRequiredService<IConfigurationService>()));
        }

        public static void RegisterEmail(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
            
            builder.Services.AddSingleton(serviceProvider =>
            {
                var configService = serviceProvider.GetRequiredService<IConfigurationService>();
                var connectionString = configService.GetAcsEmailConnectionString();
                return new EmailClient(connectionString);
            });
        }

        public static void RegisterForAuthentication(this WebApplicationBuilder builder)
        {
            var authConfigs = builder.Configuration.GetSection("Auth0").Get<AuthSettings>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
              .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, c =>
              {
                  c.Authority = authConfigs!.Authority;
                  c.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                  {
                      ValidAudience = authConfigs.Audience,
                      ValidIssuer = authConfigs.Authority,
                      NameClaimType = ClaimTypes.NameIdentifier,
                      RoleClaimType = ClaimTypes.Role
                  };
              });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("test:fail", policy => policy.RequireClaim("permissions", "test:fail"));
                options.AddPolicy("admin", policy => policy.RequireClaim("permissions", "read:users"));
                options.AddPolicy("write:plants", policy => policy.RequireClaim("permissions", "write:plants"));
                options.AddPolicy("write:plant-varieties", policy => policy.RequireClaim("permissions", "write:plant-varieties"));
                options.AddPolicy("write:grow-instructions", policy => policy.RequireClaim("permissions", "write:grow-instructions"));
                options.AddPolicy("admin_or_tester", policy => policy.RequireClaim("permissions", "validate:tester", "admin"));

                options.AddPolicy("admin:plants", policy =>
                            policy.RequireAssertion(context => context.User.HasClaim(c =>
                                (c.Type == "permissions" && (c.Value == "write:plants" || c.Value == "admin" || c.Value == "validate:tester")
                                ))));
                options.AddPolicy("admin:plant-varieties", policy =>
                            policy.RequireAssertion(context => context.User.HasClaim(c =>
                                (c.Type == "permissions" && (c.Value == "write:plant-varieties" || c.Value == "admin" || c.Value == "validate:tester")
                                ))));
                options.AddPolicy("admin:grow-instructions", policy =>
                            policy.RequireAssertion(context => context.User.HasClaim(c =>
                                (c.Type == "permissions" && (c.Value == "write:grow-instructions" || c.Value == "admin" || c.Value == "validate:tester")
                                ))));
            });
        }

        public static void RegisterSwaggerForAuth(this WebApplicationBuilder builder, string apiTitle)
        {
            var authConfigs = builder.Configuration.GetSection("Auth0").Get<AuthSettings>();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    BearerFormat = "JWT",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{authConfigs!.Authority}authorize?audience={authConfigs.Audience}"),
                            TokenUrl = new Uri($"{authConfigs.Authority}oauth/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenId" },
                                { "profile", "Profile" }
                            }
                        }
                    }
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        }, new string[]{ "openid","profile"}
                    }
            });
            });
        }

        public static IApplicationBuilder UseSwaggerForAuth(this WebApplication app, IConfigurationService configurationService)
        {

            app.UseSwagger();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerUI(
                options =>
                {
                    //options.OAuthClientSecret(configurationService.GetAuthSettings().ClientSecret);
                    options.OAuthUsePkce();
                    options.OAuthClientId(configurationService.GetAuthSettings().SwaggerClientId);
                });
            }
            return app;
        }
    }
}
