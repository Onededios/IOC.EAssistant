using Microsoft.AspNetCore.Cors.Infrastructure;

namespace IOC.EAssistant.Gateway.Api.Extension;
public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseCORSPolicy(this IApplicationBuilder app, IConfiguration conf)
    {
        var corsSection = conf.GetSection("CORS:__DefaultCorsPolicy");

        if (!corsSection.Exists())
        {
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            });
            return app;
        }

        var def = corsSection.Get<CorsPolicy>();

        app.UseCors(builder =>
        {
            if (def?.Origins != null && def.Origins.Any())
            {
                if (def.Origins.Any(x => x == "*"))
                {
                    builder.AllowAnyOrigin();
                }
                else
                {
                    if (def.Origins.Any(x => x.Contains('*')))
                        builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                    builder.WithOrigins(def.Origins.ToArray());
                }
            }
            else builder.AllowAnyOrigin();

            if (def?.Methods != null && def.Methods.Any()) builder.WithMethods(def.Methods.ToArray());
            else builder.AllowAnyMethod();

            if (def?.Headers != null && def.Headers.Any()) builder.WithHeaders(def.Headers.ToArray());
            else builder.AllowAnyHeader();

            if (def?.ExposedHeaders != null && def.ExposedHeaders.Any()) builder.WithExposedHeaders(def.ExposedHeaders.ToArray());

            if (def?.SupportsCredentials == true) builder.AllowCredentials();
        });

        return app;
    }

}
