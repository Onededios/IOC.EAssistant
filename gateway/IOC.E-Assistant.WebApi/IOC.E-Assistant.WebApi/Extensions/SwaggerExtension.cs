﻿using Microsoft.OpenApi.Models;

namespace IOC.E_Assistant.WebApi.Extensions;
public static class SwaggerExtension
{
    private static readonly string version = "v1";
    private static readonly string docs = "api-docs";
    private static readonly string file = "swagger.json";
    private static readonly string prefix = "gateway";

    public static IServiceCollection AddCustomSwagger(this IServiceCollection services) => services.AddSwaggerGen(opt => opt.SwaggerDoc(version, CreateInfoApi()));


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
        Title = "IOC.E-Assistant.WebApi",
        Version = version,
        Description = "An API to manage IOC.E-Assistant",
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
