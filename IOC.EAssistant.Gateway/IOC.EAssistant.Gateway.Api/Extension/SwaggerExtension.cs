using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Nodes;

namespace IOC.EAssistant.Gateway.Api.Extension;
public static class SwaggerExtension
{
    private static readonly string version = "v1";
    private static readonly string docs = "api-docs";
    private static readonly string file = "swagger.json";
    private static readonly string prefix = "IOC.EAssistant";
    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(opt =>
        {
            opt.SwaggerDoc(version, CreateInfoApi());

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                opt.IncludeXmlComments(xmlPath);
            }

            opt.MapType<JsonObject>(() => new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                Example = new OpenApiObject()
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCustomSwaggerUI(this IApplicationBuilder app)
    {
        app.UseSwagger(opt =>
        {
            opt.RouteTemplate = $"{prefix}/{docs}/{{documentName}}/{file}";
            opt.PreSerializeFilters.Add((doc, req) =>
            {
                var protocol = req.Scheme;
                var host = req.Host.Value;

                doc.Servers = new List<OpenApiServer> { new() { Url = $"{protocol}://{host}" } };
            });
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/{prefix}/{docs}/{version}/{file}", $"API {version}");
            c.RoutePrefix = "swagger";
        });

        return app;
    }

    public static OpenApiInfo CreateInfoApi() => new OpenApiInfo
    {
        Title = "IOC.EAssistant.Gateway",
        Version = version,
        Description = "An API to manage IOC.EAssistant",
        Contact = new OpenApiContact
        {
            Email = "joel10olor@gmail.com",
            Name = "Joel"
        },
        License = new OpenApiLicense
        {
            Name = $"Copyright {DateTime.Now.Year}, Joel Olivera aka Onededios. All rights reserved."
        }
    };
}
