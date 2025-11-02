using Microsoft.AspNetCore.Cors.Infrastructure;

namespace IOC.EAssistant.Gateway.Api.Extension;
public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseCORSPolicy(this IApplicationBuilder app, IConfiguration conf)
    {
        var def = conf.GetSection("CORS:__DefaultCorsPolicy").Get<CorsPolicy>();

        app.UseCors(builder =>
        {
            if (def.Origins.Any())
            {
                if (def.Origins.Any(x => x.Contains('*'))) builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                builder.WithOrigins(def.Origins.ToArray());
            }

            if (def.Methods.Any()) builder.WithMethods(def.Methods.ToArray());
            else builder.AllowAnyMethod();

            if (def.Headers.Any()) builder.WithHeaders(def.Headers.ToArray());
            else builder.AllowAnyHeader();

            if (def.ExposedHeaders.Any()) builder.WithExposedHeaders(def.ExposedHeaders.ToArray());

            if (def.SupportsCredentials) builder.AllowCredentials();

            builder.Build();
        });

        return app;
    }

}
