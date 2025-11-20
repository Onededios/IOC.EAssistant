namespace IOC.EAssistant.Gateway.Api.Extension;
public static class ServiceCollectionCorsExtension
{
    public static IServiceCollection AddCORSPolicy(this IServiceCollection services) =>
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
