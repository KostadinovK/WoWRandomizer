using System.Text.Json;
using WoWRandomizer.Domain;

namespace WoWRandomizer.Infrastructure;

/// <summary>
/// Loads the valid combos from a JSON file grouped by race, validates it and
/// flattens it into one <see cref="Combo"/> per race + class + spec.
/// </summary>
public sealed class JsonComboProvider : IComboProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const int SpecsPerClass = 3;

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

        var classes = ToClasses(file?.Classes);

        var duplicateRace = races
            .GroupBy(race => race?.Name?.Trim() ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Key.Length > 0 && group.Count() > 1);

        if (duplicateRace is not null)
        {
            throw new ComboDataException(
                $"Race '{duplicateRace.Key}' is defined more than once. Each race must appear once and belong to exactly one faction.");
        }

        var combos = races
            .SelectMany(race => ToCombos(race, classes))
            .ToList();

        return new JsonComboProvider(combos);
    }

    private static Dictionary<string, ClassDefinition> ToClasses(List<ClassEntry?>? entries)
    {
        if (entries is null || entries.Count == 0)
        {
            throw new ComboDataException("Combo data must contain at least one class with its specs.");
        }

        var definitions = entries.Select(ToClassDefinition).ToList();

        var duplicateClass = definitions
            .GroupBy(definition => definition.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateClass is not null)
        {
            throw new ComboDataException($"Class '{duplicateClass.Key}' is defined more than once.");
        }

        return definitions.ToDictionary(definition => definition.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static ClassDefinition ToClassDefinition(ClassEntry? entry)
    {
        var name = entry?.Name?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            throw new ComboDataException("Every class must have a name.");
        }

        var specs = (entry!.Specs ?? [])
            .Select(spec => ToSpec(name, spec))
            .ToList();

        if (specs.Count != SpecsPerClass)
        {
            throw new ComboDataException(
                $"Class '{name}' must have exactly {SpecsPerClass} specs but has {specs.Count}.");
        }

        var duplicateSpec = specs
            .GroupBy(spec => spec.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateSpec is not null)
        {
            throw new ComboDataException($"Class '{name}' lists spec '{duplicateSpec.Key}' more than once.");
        }

        return new ClassDefinition(name, specs);
    }

    private static Spec ToSpec(string className, SpecEntry? entry)
    {
        var name = entry?.Name?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            throw new ComboDataException($"Class '{className}' has a spec without a name.");
        }

        var typeNames = entry!.Types;

        if (typeNames is null || typeNames.Count == 0)
        {
            throw new ComboDataException($"Spec '{name}' of class '{className}' must have at least one type.");
        }

        var types = typeNames
            .Select(typeName => ToSpecType(className, name, typeName))
            .ToList();

        if (types.Distinct().Count() != types.Count)
        {
            throw new ComboDataException($"Spec '{name}' of class '{className}' lists the same type more than once.");
        }

        return new Spec(name, types);
    }

    private static SpecType ToSpecType(string className, string specName, string? typeName)
    {
        var match = Enum.GetValues<SpecType>()
            .Cast<SpecType?>()
            .FirstOrDefault(type => string.Equals(type.ToString(), typeName?.Trim(), StringComparison.OrdinalIgnoreCase));

        return match ?? throw new ComboDataException(
            $"Spec '{specName}' of class '{className}' has an unknown type '{typeName}'. Expected 'Tank', 'Healer' or 'DPS'.");
    }

    private static IEnumerable<Combo> ToCombos(RaceEntry? race, IReadOnlyDictionary<string, ClassDefinition> definitions)
    {
        var name = race?.Name?.Trim();

        if (string.IsNullOrEmpty(name))
        {
            throw new ComboDataException("Every race must have a name.");
        }

        if (!FactionParser.TryParse(race!.Faction, out var faction))
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

        var unknownClass = classes.FirstOrDefault(characterClass => !definitions.ContainsKey(characterClass!));

        if (unknownClass is not null)
        {
            throw new ComboDataException(
                $"Race '{name}' references class '{unknownClass}' which is not defined in the classes section.");
        }

        return classes
            .Select(characterClass => definitions[characterClass!])
            .SelectMany(definition => definition.Specs
                .Select(spec => new Combo(name, definition.Name, faction, spec)));
    }

    private sealed record ClassDefinition(string Name, IReadOnlyList<Spec> Specs);

    internal sealed record ComboFile(List<ClassEntry?>? Classes, List<RaceEntry?>? Races);

    internal sealed record ClassEntry(string? Name, List<SpecEntry?>? Specs);

    internal sealed record SpecEntry(string? Name, List<string?>? Types);

    internal sealed record RaceEntry(string? Name, string? Faction, List<string?>? Classes);
}
