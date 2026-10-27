using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using WoWRandomizer.API.Models;
using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;

namespace WoWRandomizer.IntegrationTests;

public class ComboEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly IReadOnlyList<Combo> ValidCombos = JsonComboProvider
        .FromFile(Path.Combine(AppContext.BaseDirectory, "Data", "combos.json"))
        .GetAll();

    private readonly WebApplicationFactory<Program> _factory;

    public ComboEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // ---------- no faction (US-1) ----------

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

        for (var i = 0; i < 50; i++)
        {
            var body = await client.GetFromJsonAsync<ComboResponse>("/api/combo");

            AssertIsValidCombo(body);
        }
    }

    // ---------- with faction (US-2) ----------

    [Theory]
    [InlineData("horde", "Horde")]
    [InlineData("Horde", "Horde")]
    [InlineData("HORDE", "Horde")]
    [InlineData("alliance", "Alliance")]
    [InlineData("Alliance", "Alliance")]
    [InlineData("ALLIANCE", "Alliance")]
    public async Task Get_WithFaction_ReturnsOnlyCombosOfThatFaction(string query, string expectedFaction)
    {
        var client = _factory.CreateClient();

        for (var i = 0; i < 30; i++)
        {
            var body = await client.GetFromJsonAsync<ComboResponse>($"/api/combo?faction={query}");
            Assert.NotNull(body);

            AssertIsValidCombo(body);
            Assert.Equal(expectedFaction, body.Faction);
        }
    }

    [Fact]
    public async Task Get_WithEmptyFaction_BehavesLikeNoFilter()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/combo?faction=");
        var body = await response.Content.ReadFromJsonAsync<ComboResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertIsValidCombo(body);
    }

    private static void AssertIsValidCombo(ComboResponse? body)
    {
        Assert.NotNull(body);
        Assert.Contains(ValidCombos, combo =>
            combo.Race == body.Race &&
            combo.CharacterClass == body.Class &&
            combo.Faction.ToString() == body.Faction);
    }
}
