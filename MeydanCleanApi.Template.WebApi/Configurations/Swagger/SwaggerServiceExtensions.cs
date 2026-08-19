using System.Reflection;
using Microsoft.OpenApi.Models;

namespace MeydanCleanApi.Template.WebApi.Configurations.Swagger;

/// <summary>
/// Extension methods for registering Swagger generator, EndpointsApiExplorer, and OpenAPI documentation with JWT Bearer support.
/// </summary>
public static class SwaggerServiceExtensions
{
    /// <summary>
    /// Registers EndpointsApiExplorer, Swagger generator, JWT "Authorize" UI button, and XML documentation comments.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Discovers endpoint metadata for API Explorer and OpenAPI tools
        services.AddEndpointsApiExplorer();

        // Configures Swagger document generator options
        services.AddSwaggerGen(options =>
        {
            // Set API document metadata (Title, Version, Description)
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Meydan Clean API Template",
                Version = "v1",
                Description = "Production-ready, highly extensible ASP.NET Core Clean Architecture Web API template."
            });

            // Configure JWT Bearer Security Definition (adds the "Authorize" button in Swagger UI)
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT Access Token. Example: Bearer {your_token}"
            });

            // Require Bearer security for all endpoints by default in Swagger UI
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Pull the XML doc comments into Swagger so endpoint descriptions show up in the UI.
            // Without this the documentation written throughout the code never reaches the reader.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }
}
