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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
