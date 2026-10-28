using Microsoft.AspNetCore.Cors.Infrastructure;
using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;

const string WebClientCorsPolicy = "WebClient";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// The Blazor WebAssembly UI runs as a separate app, so its origin must be allowed explicitly.
// Origins come from configuration (Cors:AllowedOrigins); none are allowed unless configured.
builder.Services.AddCors();
builder.Services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, configuration) =>
{
    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

    options.AddPolicy(WebClientCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .WithMethods(HttpMethods.Get)
        .AllowAnyHeader());
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Load and validate the combo data once at startup; the app fails fast if the file is missing or invalid.
var comboFilePath = Path.Combine(
    AppContext.BaseDirectory,
    builder.Configuration["ComboData:FilePath"] ?? Path.Combine("Data", "combos.json"));

builder.Services.AddSingleton<IComboProvider>(JsonComboProvider.FromFile(comboFilePath));
builder.Services.AddSingleton(Random.Shared);
builder.Services.AddSingleton<IComboService, ComboService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "WoWRandomizer API v1"));
}

app.UseHttpsRedirection();

app.UseCors(WebClientCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();

// Makes the entry point visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }
