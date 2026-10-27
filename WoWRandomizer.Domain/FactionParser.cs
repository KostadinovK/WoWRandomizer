namespace WoWRandomizer.Domain;

/// <summary>Parses faction names. Only the exact names are accepted (case-insensitive); numbers and lists are not.</summary>
public static class FactionParser
{
    /// <summary>The accepted faction names in lower case, e.g. "horde", "alliance".</summary>
    public static IReadOnlyList<string> AcceptedValues { get; } = Enum.GetNames<Faction>()
        .Select(name => name.ToLowerInvariant())
        .ToList();

    public static bool TryParse(string? value, out Faction faction)
    {
        var trimmed = value?.Trim();

        var match = Enum.GetValues<Faction>()
            .Select(candidate => (Faction?)candidate)
            .FirstOrDefault(candidate => string.Equals(candidate.ToString(), trimmed, StringComparison.OrdinalIgnoreCase));

        faction = match ?? default;
        return match is not null;
    }
}
