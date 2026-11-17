using WoWRandomizer.Domain;
using WoWRandomizer.Infrastructure;

namespace WoWRandomizer.UnitTests;

public class JsonComboProviderTests
{
    private static string RealDataPath => Path.Combine(AppContext.BaseDirectory, "Data", "combos.json");

    // ---------- the real data file ----------

    [Fact]
    public void RealDataFile_LoadsAll168Combos_84PerFaction()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        Assert.Equal(168, combos.Count);
        Assert.Equal(84, combos.Count(combo => combo.Faction == Faction.Horde));
        Assert.Equal(84, combos.Count(combo => combo.Faction == Faction.Alliance));
    }

    [Theory]
    [InlineData("Orc", 6)]
    [InlineData("Tauren", 4)]
    [InlineData("Troll", 7)]
    [InlineData("Undead", 6)]
    [InlineData("Horde Skyborne", 5)]
    [InlineData("Gnome", 5)]
    [InlineData("Human", 7)]
    [InlineData("Night Elf", 5)]
    [InlineData("Dwarf", 6)]
    [InlineData("Alliance Skyborne", 5)]
    public void RealDataFile_EachRaceHasExpectedNumberOfClasses(string race, int expectedClassCount)
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        var classCount = combos
            .Where(combo => combo.Race == race)
            .Select(combo => combo.CharacterClass)
            .Distinct()
            .Count();

        Assert.Equal(expectedClassCount, classCount);
    }

    [Fact]
    public void RealDataFile_EachRaceBelongsToExactlyOneFaction()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        var races = combos
            .GroupBy(combo => combo.Race)
            .ToList();

        Assert.Equal(10, races.Count);
        Assert.All(races, group => Assert.Single(group.Select(combo => combo.Faction).Distinct()));
    }

    [Fact]
    public void RealDataFile_HasNoDuplicateCombos()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        Assert.Equal(combos.Count, combos.Distinct().Count());
    }

    [Fact]
    public void RealDataFile_EveryRaceClassPairHasExactlyThreeSpecs()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        var pairs = combos
            .GroupBy(combo => (combo.Race, combo.CharacterClass))
            .ToList();

        Assert.Equal(56, pairs.Count);
        Assert.All(pairs, pair => Assert.Equal(3, pair.Count()));
    }

    [Theory]
    [InlineData("Warrior", "Arms", SpecType.DPS)]
    [InlineData("Warrior", "Fury", SpecType.DPS)]
    [InlineData("Warrior", "Protection", SpecType.Tank)]
    [InlineData("Paladin", "Holy", SpecType.Healer)]
    [InlineData("Paladin", "Protection", SpecType.Tank)]
    [InlineData("Paladin", "Retribution", SpecType.DPS)]
    [InlineData("Hunter", "Beast Mastery", SpecType.DPS)]
    [InlineData("Hunter", "Marksmanship", SpecType.DPS)]
    [InlineData("Hunter", "Survival", SpecType.DPS)]
    [InlineData("Rogue", "Assassination", SpecType.DPS)]
    [InlineData("Rogue", "Combat", SpecType.DPS)]
    [InlineData("Rogue", "Subtlety", SpecType.DPS)]
    [InlineData("Priest", "Discipline", SpecType.Healer)]
    [InlineData("Priest", "Holy", SpecType.Healer)]
    [InlineData("Priest", "Shadow", SpecType.DPS)]
    [InlineData("Shaman", "Elemental", SpecType.DPS)]
    [InlineData("Shaman", "Enhancement", SpecType.DPS)]
    [InlineData("Shaman", "Restoration", SpecType.Healer)]
    [InlineData("Mage", "Arcane", SpecType.DPS)]
    [InlineData("Mage", "Fire", SpecType.DPS)]
    [InlineData("Mage", "Frost", SpecType.DPS)]
    [InlineData("Warlock", "Affliction", SpecType.DPS)]
    [InlineData("Warlock", "Demonology", SpecType.DPS)]
    [InlineData("Warlock", "Destruction", SpecType.DPS)]
    [InlineData("Druid", "Balance", SpecType.DPS)]
    [InlineData("Druid", "Restoration", SpecType.Healer)]
    public void RealDataFile_SpecHasExpectedSingleType(string characterClass, string specName, SpecType expectedType)
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        var spec = combos
            .Where(combo => combo.CharacterClass == characterClass && combo.Spec.Name == specName)
            .Select(combo => combo.Spec)
            .Distinct()
            .Single();

        Assert.Equal([expectedType], spec.Types);
    }

    [Fact]
    public void RealDataFile_FeralCombat_IsBothTankAndDps()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        var feral = combos
            .Where(combo => combo.CharacterClass == "Druid" && combo.Spec.Name == "Feral Combat")
            .Select(combo => combo.Spec)
            .Distinct()
            .Single();

        Assert.True(feral.Is(SpecType.Tank));
        Assert.True(feral.Is(SpecType.DPS));
        Assert.False(feral.Is(SpecType.Healer));
    }

    [Fact]
    public void RealDataFile_EveryClassHasExactlyThreeSpecsWithDistinctNames()
    {
        var combos = JsonComboProvider.FromFile(RealDataPath).GetAll();

        var classes = combos
            .GroupBy(combo => combo.CharacterClass)
            .ToList();

        Assert.Equal(9, classes.Count);
        Assert.All(classes, group =>
            Assert.Equal(3, group.Select(combo => combo.Spec.Name).Distinct().Count()));
    }

    // ---------- parsing ----------

    private const string Classes = """
        "classes": [
            { "name": "Warrior", "specs": [
                { "name": "Arms", "types": ["DPS"] },
                { "name": "Fury", "types": ["DPS"] },
                { "name": "Protection", "types": ["Tank"] } ] },
            { "name": "Mage", "specs": [
                { "name": "Arcane", "types": ["DPS"] },
                { "name": "Fire", "types": ["DPS"] },
                { "name": "Frost", "types": ["DPS"] } ] },
            { "name": "Paladin", "specs": [
                { "name": "Holy", "types": ["Healer"] },
                { "name": "Protection", "types": ["Tank"] },
                { "name": "Retribution", "types": ["DPS"] } ] }
        ]
        """;

    private static string WithClasses(string racesJson) => "{ " + Classes + ", \"races\": " + racesJson + " }";

    [Fact]
    public void FromJson_ExpandsEveryRaceClassPairIntoOneComboPerSpec()
    {
        var json = WithClasses("""
            [ { "name": "Orc", "faction": "Horde", "classes": ["Warrior", "Mage"] },
              { "name": "Human", "faction": "Alliance", "classes": ["Paladin"] } ]
            """);

        var combos = JsonComboProvider.FromJson(json).GetAll();

        Assert.Equal(9, combos.Count);
        Assert.Contains(new Combo("Orc", "Warrior", Faction.Horde, new Spec("Fury", SpecType.DPS)), combos);
        Assert.Contains(new Combo("Orc", "Warrior", Faction.Horde, new Spec("Protection", SpecType.Tank)), combos);
        Assert.Contains(new Combo("Orc", "Mage", Faction.Horde, new Spec("Frost", SpecType.DPS)), combos);
        Assert.Contains(new Combo("Human", "Paladin", Faction.Alliance, new Spec("Holy", SpecType.Healer)), combos);
    }

    [Fact]
    public void FromJson_SpecWithSeveralTypes_KeepsAllTypesAsOneSpec()
    {
        const string json = """
            { "classes": [ { "name": "Druid", "specs": [
                { "name": "Balance", "types": ["DPS"] },
                { "name": "Feral Combat", "types": ["Tank", "DPS"] },
                { "name": "Restoration", "types": ["Healer"] } ] } ],
              "races": [ { "name": "Tauren", "faction": "Horde", "classes": ["Druid"] } ] }
            """;

        var combos = JsonComboProvider.FromJson(json).GetAll();

        var feral = Assert.Single(combos, combo => combo.Spec.Name == "Feral Combat");
        Assert.Equal(3, combos.Count);
        Assert.Equal([SpecType.Tank, SpecType.DPS], feral.Spec.Types);
    }

    [Fact]
    public void FromJson_FactionIsCaseInsensitive()
    {
        var json = WithClasses("""[ { "name": "Orc", "faction": "hORDE", "classes": ["Warrior"] } ]""");

        var combos = JsonComboProvider.FromJson(json).GetAll();

        Assert.All(combos, combo => Assert.Equal(Faction.Horde, combo.Faction));
    }

    [Fact]
    public void FromJson_TrimsWhitespaceAndIgnoresCaseOfClassAndTypeNames()
    {
        const string json = """
            { "classes": [ { "name": " Warrior ", "specs": [
                { "name": " Arms ", "types": [" dps "] },
                { "name": "Fury", "types": ["DPS"] },
                { "name": "Protection", "types": ["tank"] } ] } ],
              "races": [ { "name": " Orc ", "faction": "Horde", "classes": [" warrior "] } ] }
            """;

        var combos = JsonComboProvider.FromJson(json).GetAll();

        Assert.Contains(new Combo("Orc", "Warrior", Faction.Horde, new Spec("Arms", SpecType.DPS)), combos);
        Assert.Contains(new Combo("Orc", "Warrior", Faction.Horde, new Spec("Protection", SpecType.Tank)), combos);
    }

    // ---------- validation ----------

    [Theory]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("""{ "races": [] }""")]
    [InlineData("""{ "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Warrior"] } ] }""")]
    public void FromJson_MissingRacesOrClasses_Throws(string json)
    {
        Assert.Throws<ComboDataException>(() => JsonComboProvider.FromJson(json));
    }

    [Theory]
    [InlineData("""[ { "faction": "Horde", "classes": ["Warrior"] } ]""")]
    [InlineData("""[ { "name": "Orc", "classes": ["Warrior"] } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "Elves", "classes": ["Warrior"] } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "5", "classes": ["Warrior"] } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "Horde,Alliance", "classes": ["Warrior"] } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "Horde" } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "Horde", "classes": [] } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "Horde", "classes": ["Warrior", " "] } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "Horde", "classes": ["Mage", "mage"] } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "Horde", "classes": ["Mage"] }, { "name": "orc", "faction": "Horde", "classes": ["Warrior"] } ]""")]
    [InlineData("""[ { "name": "Orc", "faction": "Horde", "classes": ["Mage"] }, { "name": "Orc", "faction": "Alliance", "classes": ["Mage"] } ]""")]
    public void FromJson_InvalidRace_Throws(string racesJson)
    {
        Assert.Throws<ComboDataException>(() => JsonComboProvider.FromJson(WithClasses(racesJson)));
    }

    [Fact]
    public void FromJson_RaceReferencesUndefinedClass_ThrowsNamingTheClass()
    {
        var json = WithClasses("""[ { "name": "Orc", "faction": "Horde", "classes": ["Warrior", "Necromancer"] } ]""");

        var exception = Assert.Throws<ComboDataException>(() => JsonComboProvider.FromJson(json));

        Assert.Contains("Necromancer", exception.Message);
    }

    [Theory]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": "Arms", "types": ["DPS"] }, { "name": "Fury", "types": ["DPS"] } ] }""", "exactly 3")]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": "Arms", "types": ["DPS"] }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] }, { "name": "Extra", "types": ["DPS"] } ] }""", "exactly 3")]
    [InlineData("""{ "name": "Warrior" }""", "exactly 3")]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": "Arms", "types": ["DPS"] }, { "name": "arms", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""", "more than once")]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": "Arms", "types": ["Bruiser"] }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""", "unknown type")]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": "Arms", "types": ["1"] }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""", "unknown type")]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": "Arms", "types": [] }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""", "at least one type")]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": "Arms" }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""", "at least one type")]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": "Arms", "types": ["DPS", "dps"] }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""", "same type more than once")]
    [InlineData("""{ "name": "Warrior", "specs": [ { "name": " ", "types": ["DPS"] }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""", "without a name")]
    [InlineData("""{ "specs": [ { "name": "Arms", "types": ["DPS"] }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""", "must have a name")]
    public void FromJson_InvalidClassDefinition_ThrowsWithAClearMessage(string classJson, string expectedMessagePart)
    {
        var json = $$"""
            { "classes": [ {{classJson}} ],
              "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Warrior"] } ] }
            """;

        var exception = Assert.Throws<ComboDataException>(() => JsonComboProvider.FromJson(json));

        Assert.Contains(expectedMessagePart, exception.Message);
    }

    [Fact]
    public void FromJson_ClassDefinedTwice_Throws()
    {
        const string warrior = """{ "name": "Warrior", "specs": [ { "name": "Arms", "types": ["DPS"] }, { "name": "Fury", "types": ["DPS"] }, { "name": "Protection", "types": ["Tank"] } ] }""";
        var json = $$"""{ "classes": [ {{warrior}}, {{warrior}} ], "races": [ { "name": "Orc", "faction": "Horde", "classes": ["Warrior"] } ] }""";

        Assert.Throws<ComboDataException>(() => JsonComboProvider.FromJson(json));
    }

    // ---------- file handling ----------

    [Fact]
    public void FromFile_MissingFile_Throws()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        Assert.Throws<ComboDataException>(() => JsonComboProvider.FromFile(missingPath));
    }

    [Fact]
    public void FromFile_ChangingTheFile_ChangesTheCombos()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            File.WriteAllText(path, WithClasses("""[ { "name": "Orc", "faction": "Horde", "classes": ["Warrior"] } ]"""));
            var before = JsonComboProvider.FromFile(path).GetAll();

            File.WriteAllText(path, WithClasses("""[ { "name": "Orc", "faction": "Horde", "classes": ["Warrior", "Mage"] } ]"""));
            var after = JsonComboProvider.FromFile(path).GetAll();

            Assert.Equal(3, before.Count);
            Assert.Equal(6, after.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
