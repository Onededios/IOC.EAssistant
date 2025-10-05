namespace CustomManager.Hairdresser.API.Extensions.ServiceCollectionCorsExtension;
public static class ServiceCollectionCorsExtension
{
    public static IServiceCollection AddDefaultCorsPolicy(this IServiceCollection services) =>
        services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

}