using System.Net;
using System.Text;
using WoWRandomizer.Shared;
using WoWRandomizer.Web.Services;

namespace WoWRandomizer.UnitTests;

public class ComboApiClientTests
{
    private const string BaseAddress = "http://api.test/";
    private const string ComboJson = """{"race":"Orc","class":"Warrior","faction":"Horde"}""";

    // ---------- request ----------

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetRandomComboAsync_WithoutFaction_CallsTheComboEndpointWithoutAQuery(string? faction)
    {
        var (client, handler) = CreateClient(_ => Json(HttpStatusCode.OK, ComboJson));

        await client.GetRandomComboAsync(faction);

        Assert.Equal([$"{BaseAddress}api/combo"], handler.RequestedUris);
    }

    [Theory]
    [InlineData("horde")]
    [InlineData("Alliance")]
    public async Task GetRandomComboAsync_WithFaction_AddsItToTheQuery(string faction)
    {
        var (client, handler) = CreateClient(_ => Json(HttpStatusCode.OK, ComboJson));

        await client.GetRandomComboAsync(faction);

        Assert.Equal([$"{BaseAddress}api/combo?faction={faction}"], handler.RequestedUris);
    }

    [Fact]
    public async Task GetRandomComboAsync_EscapesTheFactionInTheQuery()
    {
        var (client, handler) = CreateClient(_ => Json(HttpStatusCode.OK, ComboJson));

        await client.GetRandomComboAsync("horde&x=1");

        Assert.Equal([$"{BaseAddress}api/combo?faction=horde%26x%3D1"], handler.RequestedUris);
    }

    // ---------- success ----------

    [Fact]
    public async Task GetRandomComboAsync_ReturnsTheComboFromTheResponse()
    {
        var (client, _) = CreateClient(_ => Json(HttpStatusCode.OK, ComboJson));

        var combo = await client.GetRandomComboAsync();

        Assert.Equal(new ComboResponse("Orc", "Warrior", "Horde"), combo);
    }

    // ---------- errors ----------

    [Fact]
    public async Task GetRandomComboAsync_WhenTheApiReturnsAProblem_ThrowsWithItsDetail()
    {
        const string problem = """{"title":"Invalid faction","detail":"'elves' is not a valid faction. Accepted values: horde, alliance."}""";
        var (client, _) = CreateClient(_ => Json(HttpStatusCode.BadRequest, problem, "application/problem+json"));

        var exception = await Assert.ThrowsAsync<ComboApiException>(() => client.GetRandomComboAsync("elves"));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal("'elves' is not a valid faction. Accepted values: horde, alliance.", exception.Message);
    }

    [Fact]
    public async Task GetRandomComboAsync_WhenTheErrorBodyIsNotJson_ThrowsWithTheStatusCode()
    {
        var (client, _) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("boom", Encoding.UTF8, "text/plain")
        });

        var exception = await Assert.ThrowsAsync<ComboApiException>(() => client.GetRandomComboAsync());

        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
        Assert.Contains("500", exception.Message);
    }

    [Fact]
    public async Task GetRandomComboAsync_WhenTheResponseBodyIsEmpty_Throws()
    {
        var (client, _) = CreateClient(_ => Json(HttpStatusCode.OK, "null"));

        await Assert.ThrowsAsync<ComboApiException>(() => client.GetRandomComboAsync());
    }

    private static (ComboApiClient Client, StubHttpMessageHandler Handler) CreateClient(
        Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new StubHttpMessageHandler(respond);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) };

        return (new ComboApiClient(httpClient), handler);
    }

    private static HttpResponseMessage Json(HttpStatusCode statusCode, string json, string mediaType = "application/json") =>
        new(statusCode) { Content = new StringContent(json, Encoding.UTF8, mediaType) };
}
