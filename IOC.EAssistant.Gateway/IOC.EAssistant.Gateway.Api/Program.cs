using IOC.EAssistant.Gateway.Api.Extension;
using IOC.EAssistant.Gateway.Infrastructure.Implementation.Extension;
using IOC.EAssistant.Gateway.Library.Implementation.Extension;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddLibraryServices();
builder.Services.AddCORSPolicy();
builder.Services.AddControllers();
builder.Services.AddCustomSwagger();

builder.Host.ConfigureLogging(builder.Configuration);

var app = builder.Build();

app.UseCustomSwaggerUI();
app.UseRouting();
app.UseCORSPolicy(builder.Configuration);
app.MapControllers();
app.AndLoggingExtension();

await app.RunAsync();