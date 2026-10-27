using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WoWRandomizer.API.Models;
using WoWRandomizer.Infrastructure;

namespace WoWRandomizer.IntegrationTests;

public class ComboEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ComboEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsOkWithJson()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/combo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_ReturnsRaceClassAndFactionProperties()
    {
        var client = _factory.CreateClient();

        var json = await client.GetStringAsync("/api/combo");

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("race", out _));
        Assert.True(document.RootElement.TryGetProperty("class", out _));
        Assert.True(document.RootElement.TryGetProperty("faction", out _));
    }

    [Fact]
    public async Task Get_AlwaysReturnsAValidCombo()
    {
        var client = _factory.CreateClient();
        var validCombos = JsonComboProvider
            .FromFile(Path.Combine(AppContext.BaseDirectory, "Data", "combos.json"))
            .GetAll();

        for (var i = 0; i < 50; i++)
        {
            var body = await client.GetFromJsonAsync<ComboResponse>("/api/combo");

            Assert.NotNull(body);
            Assert.Contains(validCombos, combo =>
                combo.Race == body.Race &&
                combo.CharacterClass == body.Class &&
                combo.Faction.ToString() == body.Faction);
        }
    }
}
