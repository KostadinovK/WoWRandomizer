using WoWRandomizer.Shared;

namespace WoWRandomizer.Web.Services;

/// <summary>Typed client for the WoWRandomizer API.</summary>
public interface IComboApiClient
{
    /// <summary>Calls <c>GET /api/combo</c> and returns a random valid race + class combo.</summary>
    /// <param name="faction">Optional faction (<c>horde</c> or <c>alliance</c>, case-insensitive). When null or empty, any faction can be returned.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <exception cref="ComboApiException">The API answered with an error status, for example 400 for an invalid faction.</exception>
    Task<ComboResponse> GetRandomComboAsync(string? faction = null, CancellationToken cancellationToken = default);
}
