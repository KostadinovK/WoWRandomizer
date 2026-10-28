using System.Net.Http.Json;
using System.Text.Json;
using WoWRandomizer.Shared;

namespace WoWRandomizer.Web.Services;

public sealed class ComboApiClient : IComboApiClient
{
    private const string ComboPath = "api/combo";

    private readonly HttpClient _httpClient;

    public ComboApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ComboResponse> GetRandomComboAsync(string? faction = null, CancellationToken cancellationToken = default)
    {
        var path = string.IsNullOrWhiteSpace(faction)
            ? ComboPath
            : $"{ComboPath}?faction={Uri.EscapeDataString(faction)}";

        using var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ComboApiException(response.StatusCode, await ReadErrorMessageAsync(response, cancellationToken));
        }

        var combo = await response.Content.ReadFromJsonAsync<ComboResponse>(cancellationToken);

        return combo ?? throw new ComboApiException(response.StatusCode, "The API returned an empty response.");
    }

    /// <summary>Uses the "detail" of a problem-details body (for example the 400 for an invalid faction), or the status line.</summary>
    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(cancellationToken);

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                return problem.Detail;
            }
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            // Not a JSON problem-details body; fall back to the status code below.
        }

        return $"The API returned status code {(int)response.StatusCode}.";
    }

    private sealed record ProblemResponse(string? Title, string? Detail);
}
