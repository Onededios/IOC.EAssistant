using IOC.E_Assistant.WebApi.Extensions;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddInfraestructureServices(builder.Configuration);
builder.Services.AddLibraryServices();
builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddCustomSwagger();

builder.Logging.AddSerilog(
    new LoggerConfiguration()
        .WriteTo.Console(new DevFormatter())
        .CreateLogger()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDeveloperExceptionPage();
app.UseCustomSwaggerUI();
app.UseRouting();
app.UseDefaultCorsPolicy(builder.Configuration);
app.MapControllers();

app.Run();

app.Logger.LogWarning("Application Started");