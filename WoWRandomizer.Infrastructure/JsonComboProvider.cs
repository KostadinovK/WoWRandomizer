using System.Text.Json;
using WoWRandomizer.Domain;

namespace WoWRandomizer.Infrastructure;

/// <summary>
/// Loads the valid combos from a JSON file grouped by race, validates it and
/// flattens it into one <see cref="Combo"/> per race + class pair.
/// </summary>
public sealed class JsonComboProvider : IComboProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IReadOnlyList<Combo> _combos;

    private JsonComboProvider(IReadOnlyList<Combo> combos)
    {
        _combos = combos;
    }

    public IReadOnlyList<Combo> GetAll() => _combos;

    public static JsonComboProvider FromFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new ComboDataException($"Combo data file was not found: '{path}'.");
        }

        return FromJson(File.ReadAllText(path));
    }

    public static JsonComboProvider FromJson(string json)
    {
        ComboFile? file;

        try
        {
            file = JsonSerializer.Deserialize<ComboFile>(json, SerializerOptions);
        }
        catch (JsonException ex)
        {
            throw new ComboDataException($"Combo data is not valid JSON: {ex.Message}", ex);
        }

        var races = file?.Races;

        if (races is null || races.Count == 0)
        {
            throw new ComboDataException("Combo data must contain at least one race.");
        }

        var combos = races
            .SelectMany(ToCombos)
            .ToList();

        var duplicateRace = races
            .GroupBy(race => race.Name!.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateRace is not null)
        {
            throw new ComboDataException(
                $"Race '{duplicateRace.Key}' is defined more than once. Each race must appear once and belong to exactly one faction.");
        }

        return new JsonComboProvider(combos);
    }

    private static IEnumerable<Combo> ToCombos(RaceEntry race)
    {
        var name = race.Name?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            throw new ComboDataException("Every race must have a name.");
        }

        if (!FactionParser.TryParse(race.Faction, out var faction))
        {
            throw new ComboDataException(
                $"Race '{name}' has an unknown faction '{race.Faction}'. Expected 'Horde' or 'Alliance'.");
        }

        var classes = race.Classes?
            .Select(characterClass => characterClass?.Trim())
            .ToList();

        if (classes is null || classes.Count == 0)
        {
            throw new ComboDataException($"Race '{name}' must have at least one class.");
        }

        if (classes.Any(string.IsNullOrEmpty))
        {
            throw new ComboDataException($"Race '{name}' has a blank class name.");
        }

        var duplicateClass = classes
            .GroupBy(characterClass => characterClass!, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateClass is not null)
        {
            throw new ComboDataException($"Race '{name}' lists class '{duplicateClass.Key}' more than once.");
        }

        return classes.Select(characterClass => new Combo(name, characterClass!, faction));
    }

    internal sealed record ComboFile(List<RaceEntry>? Races);

    internal sealed record RaceEntry(string? Name, string? Faction, List<string?>? Classes);
}
