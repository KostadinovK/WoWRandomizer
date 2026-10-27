using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace WoWRandomizer.IntegrationTests;

/// <summary>The OpenAPI document and Swagger UI are only exposed in the Development environment.</summary>
public class OpenApiDocumentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OpenApiDocumentTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
    }

    [Fact]
    public async Task Document_ContainsTheComboEndpointWithAnOptionalFactionQueryParameter()
    {
        using var document = await GetDocumentAsync();

        var faction = GetFactionParameter(document);

        Assert.Equal("query", faction.GetProperty("in").GetString());
        Assert.False(faction.TryGetProperty("required", out var required) && required.GetBoolean());
    }

    [Fact]
    public async Task FactionParameter_HasADescriptionAndAnExample()
    {
        using var document = await GetDocumentAsync();

        var faction = GetFactionParameter(document);

        Assert.True(faction.TryGetProperty("description", out var description));
        Assert.Contains("alliance", description.GetString());
        Assert.True(faction.TryGetProperty("example", out var example));
        Assert.Equal("horde", example.GetString());
    }

    [Fact]
    public async Task Document_DescribesTheOkAndBadRequestResponses()
    {
        using var document = await GetDocumentAsync();

        var responses = GetOperation(document).GetProperty("responses");

        Assert.True(responses.TryGetProperty("200", out var ok));
        Assert.True(ok.GetProperty("content").TryGetProperty("application/json", out _));
        Assert.True(responses.TryGetProperty("400", out var badRequest));
        Assert.True(badRequest.GetProperty("content").TryGetProperty("application/problem+json", out _));
    }

    [Fact]
    public async Task ComboResponseSchema_HasRaceClassAndFactionProperties()
    {
        using var document = await GetDocumentAsync();

        var properties = document.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("ComboResponse")
            .GetProperty("properties");

        Assert.True(properties.TryGetProperty("race", out _));
        Assert.True(properties.TryGetProperty("class", out _));
        Assert.True(properties.TryGetProperty("faction", out _));
    }

    [Fact]
    public async Task SwaggerUi_IsServed()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<JsonDocument> GetDocumentAsync()
    {
        var client = _factory.CreateClient();
        var json = await client.GetStringAsync("/openapi/v1.json");

        return JsonDocument.Parse(json);
    }

    private static JsonElement GetOperation(JsonDocument document) =>
        document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/Combo")
            .GetProperty("get");

    private static JsonElement GetFactionParameter(JsonDocument document) =>
        GetOperation(document)
            .GetProperty("parameters")
            .EnumerateArray()
            .Single(parameter => parameter.GetProperty("name").GetString() == "faction");
}
