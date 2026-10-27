using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();

// Makes the entry point visible to the integration tests (WebApplicationFactory<Program>).
public partial class Program { }
