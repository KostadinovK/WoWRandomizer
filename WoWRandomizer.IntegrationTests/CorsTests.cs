using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace WoWRandomizer.IntegrationTests;

/// <summary>The Web UI runs as a separate app, so the API must allow its configured origin.</summary>
public class CorsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AllowedOrigin = "https://web.example.test";
    private const string OtherOrigin = "https://evil.example.test";
    private const string AllowOriginHeader = "Access-Control-Allow-Origin";

    private readonly WebApplicationFactory<Program> _factory;

    public CorsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = AllowedOrigin
            })));
    }

    [Fact]
    public async Task Get_FromAllowedOrigin_ReturnsTheAllowOriginHeader()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/combo");
        request.Headers.Add("Origin", AllowedOrigin);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(AllowedOrigin, Assert.Single(response.Headers.GetValues(AllowOriginHeader)));
    }

    [Fact]
    public async Task Get_FromOtherOrigin_DoesNotReturnTheAllowOriginHeader()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/combo");
        request.Headers.Add("Origin", OtherOrigin);

        var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains(AllowOriginHeader));
    }

    [Fact]
    public async Task Preflight_FromAllowedOrigin_IsAccepted()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/combo");
        request.Headers.Add("Origin", AllowedOrigin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(AllowedOrigin, Assert.Single(response.Headers.GetValues(AllowOriginHeader)));
    }
}
